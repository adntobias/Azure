import os
from pathlib import Path

BASE_URL = os.getenv("LMSTUDIO_BASE_URL", "http://127.0.0.1:1234/v1")
API_KEY = os.getenv("LMSTUDIO_API_KEY", "lm-studio")  # LM Studio ignores the value

# Muss der Modell-ID in LM Studio entsprechen (siehe `python main.py models`).
EMBEDDING_MODEL = os.getenv("EMBEDDING_MODEL", "text-embedding-embeddinggemma-300m")
CHAT_MODEL = os.getenv("CHAT_MODEL", "")  # für Reranking und Antwort; leer = erstes geladenes LLM

CHUNK_SIZE = int(os.getenv("CHUNK_SIZE", "1000"))
CHUNK_OVERLAP = int(os.getenv("CHUNK_OVERLAP", "150"))
BATCH_SIZE = int(os.getenv("BATCH_SIZE", "16"))

OUTPUT_DIR = Path(os.getenv("EMBEDDINGS_DIR", "embeddings"))
