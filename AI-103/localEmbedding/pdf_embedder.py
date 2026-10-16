import json
from pathlib import Path

from openai import OpenAI
from pypdf import PdfReader

import config


def get_client() -> OpenAI:
    return OpenAI(base_url=config.BASE_URL, api_key=config.API_KEY)


# [INDEX 6/6 und SEARCH 5/7] Kernfunktion: Text -> Vektor.
# Schickt die Texte in Batches an LM Studio (/v1/embeddings) und gibt je Text einen Vektor zurück.
# Wird sowohl beim Indexieren (alle Chunks) als auch bei der Suche (nur die Frage) genutzt;
# beide müssen dasselbe Modell verwenden, sonst sind die Vektoren nicht vergleichbar.
def embed_texts(client: OpenAI, texts: list[str], model: str) -> list[list[float]]:
    vectors: list[list[float]] = []
    for i in range(0, len(texts), config.BATCH_SIZE):
        batch = [t.replace("\n", " ") for t in texts[i : i + config.BATCH_SIZE]]
        response = client.embeddings.create(input=batch, model=model)
        vectors.extend(item.embedding for item in sorted(response.data, key=lambda d: d.index))
    return vectors


# [INDEX 5/6] Teilt den Text einer Seite in Chunks fester Länge (CHUNK_SIZE) mit Überlappung.
# Whitespace wird zuerst normalisiert; die Überlappung verhindert, dass ein Satz an der Chunk-Grenze
# seinen Zusammenhang verliert. Chunks sind die Einheiten, die später gesucht werden.
def split_text(text: str, size: int, overlap: int) -> list[str]:
    text = " ".join(text.split())
    if not text:
        return []
    step = max(size - overlap, 1)
    return [text[start : start + size] for start in range(0, len(text), step)]


# [INDEX 4/6] Liest eine PDF seitenweise (pypdf) und ruft je Seite split_text auf.
# Jeder Chunk merkt sich seine Seitenzahl, damit die Suche später die Fundstelle anzeigen kann.
def extract_chunks(pdf_path: Path) -> list[dict]:
    chunks: list[dict] = []
    for page_no, page in enumerate(PdfReader(pdf_path).pages, start=1):
        for piece in split_text(page.extract_text() or "", config.CHUNK_SIZE, config.CHUNK_OVERLAP):
            chunks.append({"page": page_no, "text": piece})
    return chunks


# [INDEX 3/6] Verarbeitet eine einzelne PDF vollständig. Ablauf:
#   a) bereits indexierte PDFs überspringen (außer --force)
#   b) Chunks extrahieren (-> extract_chunks)
#   c) Vektoren erzeugen (-> embed_texts) und den Chunks zuordnen
#   d) Ergebnis als embeddings/<name>.json speichern - das ist der "Index", den die Suche lädt
def index_pdf(client: OpenAI, pdf_path: Path, model: str, force: bool = False) -> Path | None:
    out_path = config.OUTPUT_DIR / f"{pdf_path.stem}.json"
    if out_path.exists() and not force:
        print(f"Überspringe (bereits indexiert): {pdf_path.name}")
        return None

    chunks = extract_chunks(pdf_path)
    if not chunks:
        print(f"Kein Text gefunden (gescannte PDF?): {pdf_path.name}")
        return None

    vectors = embed_texts(client, [c["text"] for c in chunks], model)
    for chunk, vector in zip(chunks, vectors):
        chunk["embedding"] = vector

    config.OUTPUT_DIR.mkdir(parents=True, exist_ok=True)
    out_path.write_text(
        json.dumps({"source": pdf_path.name, "model": model, "chunks": chunks}, ensure_ascii=False),
        encoding="utf-8",
    )
    print(f"{pdf_path.name}: {len(chunks)} Chunks, Dimension {len(vectors[0])} -> {out_path}")
    return out_path


# [INDEX 2/6] Sammelt die zu indexierenden PDFs (einzelne Datei oder Ordner rekursiv)
# und ruft index_pdf nacheinander für jede auf.
def index_path(client: OpenAI, path: Path, model: str, force: bool = False) -> None:
    pdfs = sorted(path.rglob("*.pdf")) if path.is_dir() else [path]
    if not pdfs:
        print(f"Keine PDFs gefunden in {path}")
    for pdf in pdfs:
        index_pdf(client, pdf, model, force)
