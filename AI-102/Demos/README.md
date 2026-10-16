# Multi-Language Translation and Speech Processing

Dieses Programm demonstriert die Integration von Azure AI Services für Text-Übersetzung und Speech-Verarbeitung.

## Funktionsweise

Das Programm führt folgende Schritte aus:

1. **Text-Eingabe**: Benutzer gibt Text in einer beliebigen Sprache ein
2. **Text-Übersetzung**: Der Text wird automatisch ins Englische übersetzt (Azure AI Translator)
3. **Speech-Synthese**: Der englische Text wird als Audio-Datei gesprochen (Text-to-Speech)
4. **Speech-Übersetzung**: Die englische Audio wird ins Deutsche übersetzt (Speech-to-Text mit Translation)

## Voraussetzungen

### Azure Resources

Sie benötigen zwei Azure Resources:

1. **Azure AI Translator** (für Text-Übersetzung)
   - Im Azure Portal: Suchen Sie nach "Translator"
   - Erstellen Sie eine neue Ressource
   - Notieren Sie: Key und Region

2. **Azure AI Speech** (für Speech-Services)
   - Im Azure Portal: Suchen Sie nach "Speech Services"
   - Erstellen Sie eine neue Ressource
   - Notieren Sie: Key und Region

### Python-Pakete

```bash
pip install -r requirements.txt
```

## Konfiguration

1. Bearbeiten Sie die `.env` Datei
2. Tragen Sie Ihre Azure Credentials ein:

```env
TRANSLATOR_KEY=ihr_translator_schlüssel
TRANSLATOR_REGION=ihre_region (z.B. westeurope)

SPEECH_KEY=ihr_speech_schlüssel
SPEECH_REGION=ihre_region (z.B. westeurope)
```

## Verwendung

```bash
python speech.py
```

### Beispiel-Session

```
Multi-Language Translation and Speech Processing
================================================================
Services initialized successfully!
Text Translation Region: westeurope
Speech Service Region: westeurope

Enter text in any language (or 'quit' to exit):
> Bonjour, comment allez-vous?

[Step 1] Translating to English...
   Detected language: fr (confidence: 1.00)
   English translation: Hello, how are you?

[Step 2] Synthesizing English speech...
   Synthesizing: 'Hello, how are you?'
   Speech saved to: english_speech.wav

[Step 3] Translating speech to German...
   Processing audio file: english_speech.wav
   Recognized English: Hello, how are you?
   German translation: Hallo, wie geht es Ihnen?

================================================================
FINAL RESULT:
Original: Bonjour, comment allez-vous?
English:  Hello, how are you?
German:   Hallo, wie geht es Ihnen?
================================================================
```

## Unterstützte Sprachen

### Text-Übersetzung (Input)
- Automatische Spracherkennung für über 100 Sprachen
- Beispiele: Deutsch (de), Französisch (fr), Spanisch (es), Italienisch (it), Chinesisch (zh), Japanisch (ja), etc.

### Speech Services
- **Text-to-Speech**: Englisch (en-US) mit Jenny Neural voice
- **Speech Translation**: Englisch → Deutsch

## Technische Details

### Verwendete Azure Services

1. **Azure AI Translator**
   - Text Translation API
   - Automatische Spracherkennung
   - Übersetzung zu Englisch

2. **Azure AI Speech**
   - Text-to-Speech (Neural Voices)
   - Speech Translation API
   - WAV Audio-Format

### Code-Struktur

- `main()`: Hauptschleife und Orchestrierung
- `translate_to_english()`: Text-Übersetzung zu Englisch
- `synthesize_speech()`: Text-to-Speech Synthese
- `translate_speech_to_german()`: Speech-to-Text mit Übersetzung zu Deutsch

## Fehlerbehandlung

Das Programm enthält umfassende Fehlerbehandlung für:
- Fehlende Konfiguration
- API-Fehler
- Netzwerkprobleme
- Audio-Verarbeitungsfehler

## Hinweise

- Die temporäre Audio-Datei (`english_speech.wav`) wird nach jedem Durchlauf automatisch gelöscht
- Bei Netzwerkproblemen werden aussagekräftige Fehlermeldungen angezeigt
- Das Programm läuft in einer Schleife, bis 'quit' eingegeben wird

## Kostenoptimierung

- Verwenden Sie die Free Tier (F0) für Tests
- Azure AI Translator Free: 2M Zeichen/Monat
- Azure AI Speech Free: 5 Stunden Audio/Monat

## Weitere Ressourcen

- [Azure AI Translator Documentation](https://learn.microsoft.com/azure/ai-services/translator/)
- [Azure AI Speech Documentation](https://learn.microsoft.com/azure/ai-services/speech-service/)
- [Supported Languages](https://learn.microsoft.com/azure/ai-services/translator/language-support)
