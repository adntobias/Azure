# GESAMTABLAUF (Zahlen in den Kommentaren der Funktionen = Reihenfolge)
#
#   INDEX  (python main.py index <pfad>)   - einmalig je PDF, Ergebnis: embeddings/<name>.json
#     [1] cmd_index -> [2] index_path -> [3] index_pdf -> [4] extract_chunks -> [5] split_text
#         -> [6] embed_texts (LM Studio /v1/embeddings) -> JSON speichern (zurück in index_pdf)
#
#   SEARCH (python main.py search "...")   - liest nur die JSON-Dateien, PDFs werden nicht mehr angefasst
#     [1] cmd_search -> [2] search -> [3] vector_search -> [4] load_index
#         -> [5] embed_texts (Frage) -> [6] rerank (optional, --rerank) -> [7] answer (LLM-Antwort)

import argparse
from pathlib import Path

import openai

import config
from pdf_embedder import get_client, index_path
from search import answer, search


def cmd_models(client, args):
    for m in client.models.list().data:
        print(m.id)


# [INDEX 1/6] Einstiegspunkt für `index`: reicht Pfad und Optionen an index_path weiter.
def cmd_index(client, args):
    index_path(client, Path(args.path), args.model, args.force)


# [SEARCH 1/7] Einstiegspunkt für `search`. Ablauf:
#   a) search() liefert die Top-K Treffer (Vektorsuche, optional mit Rerank)
#   b) Treffer werden mit Quelle, Seite und Scores ausgegeben
#   c) answer() erzeugt aus diesen Treffern die LLM-Antwort und hängt sie ans Ende an
def cmd_search(client, args):
    hits = search(client, args.query, args.model, args.k, args.rerank, args.candidates)
    for rank, hit in enumerate(hits, start=1):
        rr = f" | rerank {hit.rerank_score:.1f}" if hit.rerank_score is not None else ""
        print(f"\n#{rank}  {hit.source}, S. {hit.page}  (cosine {hit.score:.3f}{rr})")
        print(hit.text[:400] + ("…" if len(hit.text) > 400 else ""))

    if not args.no_answer:
        print("\n" + "=" * 60 + "\nAntwort:\n" + answer(client, args.query, hits))


def main() -> None:
    parser = argparse.ArgumentParser(description="PDF-Embeddings mit LM Studio")
    parser.add_argument("--model", default=config.EMBEDDING_MODEL, help="Embedding-Modell-ID in LM Studio")
    sub = parser.add_subparsers(dest="command", required=True)

    sub.add_parser("models", help="Modelle in LM Studio auflisten").set_defaults(func=cmd_models)

    p_index = sub.add_parser("index", help="PDF(s) indexieren")
    p_index.add_argument("path", help="PDF-Datei oder Ordner")
    p_index.add_argument("--force", action="store_true", help="Bereits indexierte PDFs neu erzeugen")
    p_index.set_defaults(func=cmd_index)

    p_search = sub.add_parser("search", help="Semantische Suche")
    p_search.add_argument("query")
    p_search.add_argument("-k", type=int, default=5, help="Anzahl Treffer")
    p_search.add_argument("--rerank", action="store_true", help="Treffer mit LLM neu bewerten")
    p_search.add_argument("--no-answer", action="store_true", help="Keine LLM-Antwort generieren")
    p_search.add_argument("--candidates", type=int, default=20, help="Kandidaten für den Reranker")
    p_search.set_defaults(func=cmd_search)

    args = parser.parse_args()
    try:
        args.func(get_client(), args)
    except openai.APIConnectionError:
        raise SystemExit(f"LM Studio nicht erreichbar unter {config.BASE_URL}. Server gestartet?")
    except openai.NotFoundError as e:
        raise SystemExit(f"Modell nicht gefunden/geladen: {e}")


if __name__ == "__main__":
    main()
