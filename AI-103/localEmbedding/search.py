import json
import re
from dataclasses import dataclass

import numpy as np
from openai import OpenAI

import config
from pdf_embedder import embed_texts

RERANK_PROMPT = (
    "Bewerte, wie gut der Textausschnitt die Frage beantwortet. "
    "Antworte ausschließlich mit einer ganzen Zahl von 0 (irrelevant) bis 10 (beantwortet die Frage vollständig).\n\n"
    "Frage: {query}\n\nTextausschnitt:\n{passage}\n\nBewertung:"
)

ANSWER_PROMPT = (
    "Answer the question using only the text excerpts below. "
    "Reply in the same language as the question and cite the sources used as [number]. "
    "If the excerpts do not contain the answer, say so.\n\n"
    "Excerpts:\n{context}\n\nQuestion: {query}\n\nAnswer:"
)


@dataclass
class Hit:
    source: str
    page: int
    text: str
    score: float
    rerank_score: float | None = None


# [SEARCH 4/7] Lädt alle embeddings/*.json in eine Metadatenliste (Quelle, Seite, Text) und eine
# NumPy-Matrix (eine Zeile je Chunk). Dateien, die mit einem anderen Embedding-Modell erzeugt
# wurden, werden übersprungen, weil ihre Vektoren nicht mit der Frage vergleichbar wären.
def load_index(model: str) -> tuple[list[dict], np.ndarray]:
    meta: list[dict] = []
    vectors: list[list[float]] = []
    for path in sorted(config.OUTPUT_DIR.glob("*.json")):
        data = json.loads(path.read_text(encoding="utf-8"))
        if data["model"] != model:
            print(f"Warnung: {path.name} wurde mit '{data['model']}' erzeugt, übersprungen.")
            continue
        for chunk in data["chunks"]:
            meta.append({"source": data["source"], "page": chunk["page"], "text": chunk["text"]})
            vectors.append(chunk["embedding"])
    if not vectors:
        raise SystemExit("Kein Index gefunden. Zuerst `python main.py index <pfad>` ausführen.")
    return meta, np.asarray(vectors, dtype=np.float32)


# [SEARCH 3/7] Stufe 1 der Suche (schnell, grob): semantische Ähnlichkeit. Ablauf:
#   a) Index laden (-> load_index)
#   b) Frage einbetten (-> embed_texts)
#   c) Cosine-Similarity der Frage zu allen Chunks berechnen
#   d) die top_n ähnlichsten Chunks als Hit zurückgeben
def vector_search(client: OpenAI, query: str, model: str, top_n: int) -> list[Hit]:
    meta, matrix = load_index(model)
    query_vec = np.asarray(embed_texts(client, [query], model)[0], dtype=np.float32)
    scores = (matrix @ query_vec) / (np.linalg.norm(matrix, axis=1) * np.linalg.norm(query_vec) + 1e-12)
    best = np.argsort(scores)[::-1][:top_n]
    return [Hit(score=float(scores[i]), **meta[i]) for i in best]


# Hilfsfunktion für rerank und answer: wählt das Chat-Modell (CHAT_MODEL oder erstes Nicht-Embedding-Modell).
def resolve_chat_model(client: OpenAI) -> str:
    if config.CHAT_MODEL:
        return config.CHAT_MODEL
    for m in client.models.list().data:
        if "embed" not in m.id.lower():
            return m.id
    raise SystemExit("Kein Chat-Modell in LM Studio gefunden. CHAT_MODEL setzen oder ein Modell laden.")


# [SEARCH 6/7] Stufe 2 der Suche (langsamer, genauer), nur mit --rerank. Ablauf:
#   a) das Chat-Modell bewertet jeden Kandidaten einzeln mit 0-10 ("beantwortet das die Frage?")
#   b) Sortierung nach diesem Score (bei Gleichstand nach Cosine)
# Korrigiert Treffer, die zwar ähnlich klingen, die Frage aber nicht beantworten (z. B. reine Linkfolien).
def rerank(client: OpenAI, query: str, hits: list[Hit]) -> list[Hit]:
    model = resolve_chat_model(client)
    for hit in hits:
        response = client.chat.completions.create(
            model=model,
            temperature=0,
            max_tokens=8,
            messages=[{"role": "user", "content": RERANK_PROMPT.format(query=query, passage=hit.text)}],
        )
        match = re.search(r"\d+(?:[.,]\d+)?", response.choices[0].message.content or "")
        hit.rerank_score = min(float(match.group().replace(",", ".")), 10.0) if match else 0.0
    return sorted(hits, key=lambda h: (h.rerank_score, h.score), reverse=True)


# [SEARCH 7/7] Letzter Schritt: erzeugt die eigentliche Antwort. Ablauf:
#   a) die Treffer werden nummeriert als Kontext zusammengesetzt
#   b) das Chat-Modell antwortet nur anhand dieses Kontexts und zitiert die Nummern
# Das ist der "Generation"-Teil von RAG; Suche und Rerank liefern nur das Material dafür.
def answer(client: OpenAI, query: str, hits: list[Hit]) -> str:
    context = "\n\n".join(f"[{i}] ({h.source}, S. {h.page})\n{h.text}" for i, h in enumerate(hits, start=1))
    response = client.chat.completions.create(
        model=resolve_chat_model(client),
        temperature=0.2,
        messages=[{"role": "user", "content": ANSWER_PROMPT.format(query=query, context=context)}],
    )
    return (response.choices[0].message.content or "").strip()


# [SEARCH 2/7] Orchestriert die Trefferermittlung. Ablauf:
#   a) vector_search holt Kandidaten (mit Rerank mehr, z. B. 20, sonst direkt k)
#   b) optional rerank sortiert die Kandidaten neu
#   c) die besten k werden an cmd_search zurückgegeben
def search(client: OpenAI, query: str, model: str, k: int = 5, use_rerank: bool = False, candidates: int = 20) -> list[Hit]:
    hits = vector_search(client, query, model, max(candidates, k) if use_rerank else k)
    if use_rerank:
        hits = rerank(client, query, hits)
    return hits[:k]
