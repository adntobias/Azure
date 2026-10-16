# PDF-Embeddings mit LM Studio

## Voraussetzungen
- LM Studio: Embedding-Modell [embeddinggemma-300M](https://huggingface.co/ggml-org/embeddinggemma-300M-GGUF) laden, lokalen Server starten (Standard `http://127.0.0.1:1234/v1`).
- Ein Chat-Modell (z. B. `google/gemma-3-4b`) laden: wird für die Antwort und für `--rerank` benötigt.
- Modell-IDs anzeigen: `python main.py models`

## Setup
```powershell
py -3.13 -m venv .venv
.\.venv\Scripts\Activate.ps1
pip install -r requirements.txt
```

## Nutzung
```powershell
$env:PYTHONUTF8 = "1"                       # Umlaute korrekt in der Konsole

python main.py models                       # geladene Modelle in LM Studio
python main.py index .\pdfs                 # embedden: Ordner oder einzelne PDF
python main.py index .\pdfs --force         # bereits indexierte PDFs neu erzeugen
python main.py search "Meine Frage"         # Suche + LLM-Antwort am Ende
python main.py search "Meine Frage" -k 5 --rerank
python main.py search "Meine Frage" --no-answer
python main.py --model <embedding-id> search "Meine Frage"
```

## Parameter

Global (vor dem Unterbefehl angeben):

| Parameter | Bedeutung |
|---|---|
| `--model <id>` | Embedding-Modell in LM Studio (Standard `text-embedding-embeddinggemma-300m`). Für Indexierung und Suche dasselbe Modell nutzen. |

`index <pfad>`:

| Parameter | Bedeutung |
|---|---|
| `pfad` | PDF-Datei oder Ordner (rekursiv). |
| `--force` | Bereits indexierte PDFs neu embedden (sonst übersprungen). |

`search "<frage>"`:

| Parameter | Bedeutung |
|---|---|
| `frage` | Suchfrage in natürlicher Sprache. |
| `-k <n>` | Anzahl Treffer, die ausgegeben und als Kontext für die Antwort genutzt werden (Standard 5). |
| `--rerank` | Kandidaten zusätzlich vom Chat-Modell mit 0-10 bewerten und neu sortieren. Genauer, aber langsamer. |
| `--candidates <n>` | Anzahl Kandidaten, die der Reranker bewertet (Standard 20, nur mit `--rerank`). |
| `--no-answer` | Keine LLM-Antwort erzeugen, nur Treffer anzeigen. |

## Konfiguration (Umgebungsvariablen)

| Variable | Standard | Bedeutung |
|---|---|---|
| `LMSTUDIO_BASE_URL` | `http://127.0.0.1:1234/v1` | Adresse des LM Studio Servers. |
| `EMBEDDING_MODEL` | `text-embedding-embeddinggemma-300m` | Embedding-Modell (wie `--model`). |
| `CHAT_MODEL` | erstes Nicht-Embedding-Modell | Modell für Rerank und Antwort. |
| `CHUNK_SIZE` / `CHUNK_OVERLAP` | `1000` / `150` | Chunk-Länge und Überlappung in Zeichen (beim Indexieren). |
| `BATCH_SIZE` | `16` | Chunks pro Embedding-Anfrage. |
| `EMBEDDINGS_DIR` | `embeddings` | Ausgabeordner der JSON-Dateien. |

Embeddings liegen als JSON je PDF in `embeddings/`.
