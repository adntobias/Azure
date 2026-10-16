---
lab:
    title: 'Comprehensive Speech and Language Analysis Lab'
    description: "Build a complete audio processing application step-by-step on your local machine using Azure AI services for speech recognition, translation, and text analysis."
---

# Comprehensive Speech and Language Analysis Lab

## Overview

In this hands-on lab, you'll build a comprehensive audio processing application. You'll start with basic audio validation and progressively add more advanced capabilities using Azure AI services.

### What You'll Build

An intelligent audio processing application that combines:

- **Azure AI Speech** - For speech-to-text transcription and text-to-speech synthesis
- **Azure AI Translator** - For translating text between languages  
- **Azure AI Language** - For text analysis including sentiment, entities, and key phrases

### Learning Approach

This lab uses an **iterative development approach**. You'll build a single application file (`speech-analysis.py`) and progressively add features:

1. **Part 1**: Setup and configuration
2. **Part 2**: Audio processing and speech transcription
3. **Part 3**: Language detection and translation
4. **Part 4**: Text analysis (sentiment, entities, key phrases)
5. **Part 5**: Summarization and function calling

After each part, you'll test your application to ensure it works before moving to the next step. This incremental approach helps you understand each component and makes debugging easier.

### Final Capabilities

By the end of this lab, your application will:
1. ✓ Validate and convert audio files to Azure-compatible format
2. ✓ Transcribe speech from audio files
3. ✓ Detect the language of transcribed text
4. ✓ Translate text to multiple languages
5. ✓ Analyze sentiment, entities, and key phrases
6. ✓ Generate content summaries
7. ✓ Respond to voice commands with synthesized speech

**Estimated time**: 90 minutes

## Prerequisites

### Required Software

Before starting this lab, ensure you have the following installed on your laptop:

- **Python 3.8 or higher** ([Download](https://www.python.org/downloads/))
  - Verify: `python --version` or `python3 --version`
- **pip** (Python package installer, usually included with Python)
  - Verify: `pip --version` or `pip3 --version`
- **FFmpeg** (audio conversion tool)
  - **Windows**: Download from [ffmpeg.org](https://ffmpeg.org/download.html) or use `choco install ffmpeg`
  - Verify: `ffmpeg -version`
- **Text editor** (VS Code, PyCharm, or any editor you're comfortable with)
- **Azure subscription** with permissions to create resources

---

## Part 1: Setup and Configuration

### Step 1.1: Provision Azure AI Resources

You'll need to create three Azure AI resources. These will provide the APIs your application uses.

#### Create an Azure AI Speech Resource

1. Open the Azure portal at [https://portal.azure.com](https://portal.azure.com)
2. Sign in with your Azure subscription credentials
3. In the search bar at the top, type **Speech service** and select it
4. Click **Create** and configure:
   - **Subscription**: Your Azure subscription
   - **Resource group**: Create a new one named `speech-language-lab-rg` (or use existing)
   - **Region**: Choose a region close to you (e.g., East US, West Europe)
   - **Name**: Enter a unique name (e.g., `speech-lab-yourname-123`)
   - **Pricing tier**: Select **F0** (free) if available, otherwise **S0**
5. Click **Review + create**, then **Create**
6. Wait for deployment (1-2 minutes)
7. Click **Go to resource**
8. Navigate to **Keys and Endpoint** (left sidebar)
9. **Copy and save**:
   - **KEY 1** (you'll need this)
   - **Location/Region** (e.g., "eastus")

#### Create an Azure AI Translator Resource

1. In Azure Portal search bar, type **Translator** and select it
2. Click **Create** and configure:
   - **Subscription**: Your Azure subscription
   - **Resource group**: Use the same as above (`speech-language-lab-rg`)
   - **Region**: Same region as Speech resource
   - **Name**: Enter a unique name (e.g., `translator-lab-yourname-123`)
   - **Pricing tier**: Select **F0** (free) if available, otherwise **S1**
3. Click **Review + create**, then **Create**
4. After deployment, click **Go to resource**
5. Navigate to **Keys and Endpoint**
6. **Copy and save**:
   - **KEY 1**
   - **Location/Region**

#### Create an Azure AI Language Resource

1. In Azure Portal search bar, type **Language service** and select it
2. Click **Create** and configure:
   - **Subscription**: Your Azure subscription
   - **Resource group**: Use the same (`speech-language-lab-rg`)
   - **Region**: Same region as other resources
   - **Name**: Enter a unique name (e.g., `language-lab-yourname-123`)
   - **Pricing tier**: Select **F0** (free) if available, otherwise **S**
   - **Responsible AI Notice**: Check the box to agree
3. Click **Review + create**, then **Create**
4. After deployment, click **Go to resource**
5. Navigate to **Keys and Endpoint**
6. **Copy and save**:
   - **KEY 1**
   - **Endpoint** (full URL, e.g., `https://language-lab-yourname-123.cognitiveservices.azure.com/`)

> **💡 Tip**: Keep these keys and endpoints in a text file - you'll need them in the next step!

### Step 1.2: Set Up Local Development Environment

Now you'll create your project directory and set up Python.

#### Create Project Directory

Open a terminal/command prompt and run:

**Windows (PowerShell):**
```powershell
# Navigate to a location where you want to create the project
cd $HOME\Desktop

# Create project directory
mkdir speech-language-lab
cd speech-language-lab
```

#### Create Python Virtual Environment

A virtual environment isolates your project dependencies.

**Windows:**
```powershell
# Create virtual environment
python -m venv venv

# Activate it
.\venv\Scripts\Activate.ps1

# If you get an execution policy error, run:
# Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

You should see `(venv)` at the start of your command prompt, indicating the virtual environment is active.

#### Create Configuration File

Create a file named `.env` to store your Azure credentials:

**Windows (PowerShell):**
```powershell
New-Item .env -ItemType File
```

Add your Azure credentials to the `.env` file:

```env
SPEECH_KEY=your_speech_key_here
SPEECH_REGION=your_speech_region_here
TRANSLATOR_KEY=your_translator_key_here
TRANSLATOR_REGION=your_translator_region_here
LANGUAGE_KEY=your_language_key_here
LANGUAGE_ENDPOINT=your_language_endpoint_here
```

Save and close the file.

#### Install Python Dependencies

Create a `requirements.txt` file:

```
python-dotenv==1.0.0
azure-cognitiveservices-speech==1.47.0
azure-ai-translation-text==1.0.1
azure-ai-textanalytics==5.3.0
azure-core==1.31.0
```

Install the packages:

```bash
pip install -r requirements.txt
```

This will take 1-2 minutes. You should see each package installing successfully.

### Step 1.3: Create Test Audio Files

Before starting development, create some test audio files using Azure Speech synthesis.

Create a file named `create-samples.py`:

```python
from dotenv import load_dotenv
import os
import azure.cognitiveservices.speech as speech_sdk

load_dotenv()
speech_key = os.getenv('SPEECH_KEY')
speech_region = os.getenv('SPEECH_REGION')

speech_config = speech_sdk.SpeechConfig(speech_key, speech_region)
speech_config.speech_synthesis_voice_name = "en-US-JennyNeural"

# Different test scenarios
test_samples = [
    ("test.wav", 
     "Hello, this is a test of the audio validation system."),
    
    ("hotel-review.wav", 
     "I stayed at the Grand Hotel in Paris last week. "
     "The service was excellent and the rooms were very clean. "
     "However, the restaurant was quite expensive. "
     "Overall, I would recommend it to anyone visiting Paris."),
    
    ("time-request.wav",
     "What time is it right now?"),
]

print("Creating test audio samples...\n")

for filename, text in test_samples:
    audio_config = speech_sdk.audio.AudioOutputConfig(filename=filename)
    synthesizer = speech_sdk.SpeechSynthesizer(
        speech_config=speech_config,
        audio_config=audio_config
    )
    
    result = synthesizer.speak_text_async(text).get()
    
    if result.reason == speech_sdk.ResultReason.SynthesizingAudioCompleted:
        print(f"✓ Created: {filename}")
    else:
        print(f"✗ Failed: {filename}")

print("\n✓ Test samples created!")
```

Run it to create your test files:

```bash
python create-samples.py
```

---

## Part 2: Build the Core Application with Audio Processing and Transcription

In this part, you'll create the main application file with audio format validation, conversion, and speech transcription capabilities.

### Step 2.1: Create the Main Application File

Create a new file named `speech-analysis.py` with the complete foundation:

```python
from dotenv import load_dotenv
import os
import sys
import wave
import subprocess
import azure.cognitiveservices.speech as speech_sdk

def check_audio_format(file_path):
    """
    Check if an audio file meets Azure Speech service requirements.
    Returns: (is_valid, message)
    """
    try:
        with wave.open(file_path, 'rb') as wav_file:
            channels = wav_file.getnchannels()
            sample_width = wav_file.getsampwidth()
            frame_rate = wav_file.getframerate()
            
            print(f"  Channels: {channels} (need 1 for mono)")
            print(f"  Sample rate: {frame_rate} Hz (need 16000 Hz)")
            print(f"  Bit depth: {sample_width * 8}-bit (need 16-bit)")
            
            is_valid = (
                channels == 1 and           # Mono
                sample_width == 2 and       # 16-bit (2 bytes)
                frame_rate == 16000         # 16 kHz
            )
            
            if is_valid:
                return True, "✓ Audio format is valid for Azure Speech service."
            else:
                return False, "✗ Audio format does not meet requirements."
        
    except Exception as e:
        return False, f"✗ Error reading audio file: {str(e)}"


def convert_audio_to_speech_format(input_file, output_file=None):
    """
    Convert an audio file to Azure Speech service compatible format using ffmpeg.
    """
    if output_file is None:
        base_name = input_file.rsplit('.', 1)[0]
        output_file = f"{base_name}_converted.wav"
    
    try:
        print(f"Converting {input_file}...")
        
        command = [
            'ffmpeg',
            '-i', input_file,
            '-ar', '16000',
            '-ac', '1',
            '-sample_fmt', 's16',
            '-y',
            output_file
        ]
        
        result = subprocess.run(command, capture_output=True, text=True)
        
        if result.returncode == 0:
            print(f"✓ Successfully converted to: {output_file}")
            return output_file
        else:
            print(f"✗ Error converting audio: {result.stderr}")
            return None
            
    except FileNotFoundError:
        print("✗ Error: ffmpeg not found. Please install ffmpeg.")
        return None
    except Exception as e:
        print(f"✗ Exception during conversion: {str(e)}")
        return None


def process_audio_file(file_path):
    """
    Check audio format and convert if necessary.
    Returns the path to a valid audio file.
    """
    print(f"\n{'='*60}")
    print(f"Processing audio file: {file_path}")
    print(f"{'='*60}\n")
    
    if not os.path.exists(file_path):
        print(f"✗ File not found: {file_path}")
        return None
    
    is_valid, message = check_audio_format(file_path)
    print(f"\n{message}")
    
    if is_valid:
        print("✓ No conversion needed!")
        return file_path
    else:
        print("\n⚙ Converting audio to required format...")
        converted_file = convert_audio_to_speech_format(file_path)
        
        if converted_file:
            print("\nVerifying converted file...")
            is_valid, message = check_audio_format(converted_file)
            print(f"\n{message}")
            
            if is_valid:
                print("✓ Conversion successful!")
                return converted_file
            else:
                print("✗ Conversion failed validation.")
                return None
        return None


def transcribe_speech(audio_file, speech_config):
    """
    Transcribe speech from an audio file.
    Returns the transcribed text or None if failed.
    """
    try:
        print("\n" + "-"*60)
        print("SPEECH TRANSCRIPTION")
        print("-"*60)
        print(f"Processing: {audio_file}")
        
        audio_config = speech_sdk.AudioConfig(filename=audio_file)
        speech_recognizer = speech_sdk.SpeechRecognizer(
            speech_config=speech_config,
            audio_config=audio_config
        )
        
        print("Transcribing audio...")
        result = speech_recognizer.recognize_once_async().get()
        
        if result.reason == speech_sdk.ResultReason.RecognizedSpeech:
            print("✓ Transcription successful!")
            return result.text
            
        elif result.reason == speech_sdk.ResultReason.NoMatch:
            print("✗ No speech could be recognized in the audio.")
            return None
            
        elif result.reason == speech_sdk.ResultReason.Canceled:
            cancellation = result.cancellation_details
            print(f"✗ Speech recognition canceled: {cancellation.reason}")
            if cancellation.reason == speech_sdk.CancellationReason.Error:
                print(f"   Error details: {cancellation.error_details}")
            return None
            
    except Exception as e:
        print(f"✗ Error during transcription: {str(e)}")
        return None


def main():
    """Main application entry point"""
    os.system('cls' if os.name=='nt' else 'clear')
    
    try:
        # Get Configuration Settings
        load_dotenv()
        ai_endpoint = os.getenv('SPEECH_KEY')
        ai_region = os.getenv('SPEECH_REGION')
        
        # Get audio file from command line or use default
        audio_file = 'test.wav'
        if len(sys.argv) > 1:
            audio_file = sys.argv[1]
        
        # Configure Azure AI Speech
        speech_config = speech_sdk.SpeechConfig(ai_endpoint, ai_region)
        print('✓ Azure AI Speech configured\n')
        
        # Process audio file (validate/convert)
        valid_audio = process_audio_file(audio_file)
        
        if valid_audio:
            # Transcribe the audio
            transcribed_text = transcribe_speech(valid_audio, speech_config)
            
            # ===== PART 3+ MODIFICATIONS START HERE =====
            # In Part 3, you'll replace the code below with language detection and translation
            if transcribed_text:
                print("\n" + "="*60)
                print("TRANSCRIPTION RESULT")
                print("="*60)
                print(f"\n{transcribed_text}\n")
            else:
                print("\n✗ Transcription failed.")
            # ===== PART 3+ MODIFICATIONS END HERE =====
        else:
            print(f"\n✗ Could not process audio file.")
                
    except Exception as ex:
        print(f"\n✗ An error occurred: {ex}")


if __name__ == "__main__":
    main()
```

### Step 2.2: Test the Application

Run your application with the test file:

```bash
python speech-analysis.py test.wav
```

You should see:
1. Audio format validation
2. Speech transcription
3. The transcribed text displayed

Try with the hotel review:

```bash
python speech-analysis.py hotel-review.wav
```

---

## Part 3: Add Language Detection and Translation

Now you'll extend the application to detect languages and translate text.

### Step 3.1: Update the Application

Update `speech-analysis.py` by adding these imports at the top (after the existing imports):

```python
from azure.core.credentials import AzureKeyCredential
from azure.ai.textanalytics import TextAnalyticsClient, ExtractiveSummaryAction
from azure.ai.translation.text import TextTranslationClient
from azure.ai.translation.text.models import InputTextItem
```

Add these new functions (before the `main()` function):

```python
def detect_language(text, language_client):
    """
    Detect the language of the provided text.
    """
    try:
        print("\n" + "-"*60)
        print("LANGUAGE DETECTION")
        print("-"*60)
        
        detected_language = language_client.detect_language(documents=[text])[0]
        
        language_name = detected_language.primary_language.name
        language_iso = detected_language.primary_language.iso6391_name
        confidence = detected_language.primary_language.confidence_score
        
        print(f"Detected language: {language_name} ({language_iso})")
        print(f"Confidence: {confidence:.2%}")
        
        return language_iso
        
    except Exception as e:
        print(f"✗ Error detecting language: {str(e)}")
        return None


def translate_text(text, translator_client, source_lang='en'):
    """
    Translate text to multiple target languages.
    """
    try:
        print("\n" + "-"*60)
        print("TEXT TRANSLATION")
        print("-"*60)
        
        target_languages = ['fr', 'es', 'de', 'hi']
        
        language_names = {
            'fr': 'French', 'es': 'Spanish', 'de': 'German', 'hi': 'Hindi', 'en': 'English'
        }
        
        print("\nTranslations:")
        print("-" * 60)
        
        input_text_elements = [InputTextItem(text=text)]
        translations = {}
        
        for target_lang in target_languages:
            translation_response = translator_client.translate(
                body=input_text_elements,
                to_language=[target_lang],
                from_language=source_lang
            )
            
            if translation_response and len(translation_response) > 0:
                translation = translation_response[0]
                
                for translated_text in translation.translations:
                    lang_name = language_names.get(translated_text.to, translated_text.to)
                    print(f"\n[{lang_name}]:")
                    print(f"  {translated_text.text}")
                    translations[translated_text.to] = translated_text.text
        
        return translations
        
    except Exception as e:
        print(f"✗ Error during translation: {str(e)}")
        return None
```

Now update the `main()` function. Find this section after transcription:

```python
if transcribed_text:
    print("\n" + "="*60)
    print("TRANSCRIPTION RESULT")
    print("="*60)
    print(f"\n{transcribed_text}\n")
else:
    print("\n✗ Transcription failed.")
```

Replace it with:

```python
if transcribed_text:
    print("\n" + "="*60)
    print("TRANSCRIPTION RESULT")
    print("="*60)
    print(f"\n{transcribed_text}\n")
    
    # Configure Language and Translator services
    language_key = os.getenv('LANGUAGE_KEY')
    language_endpoint = os.getenv('LANGUAGE_ENDPOINT')
    language_credential = AzureKeyCredential(language_key)
    language_client = TextAnalyticsClient(
        endpoint=language_endpoint,
        credential=language_credential
    )
    print('✓ Azure AI Language configured')
    
    translator_key = os.getenv('TRANSLATOR_KEY')
    translator_region = os.getenv('TRANSLATOR_REGION')
    translator_credential = AzureKeyCredential(translator_key)
    translator_client = TextTranslationClient(
        credential=translator_credential,
        region=translator_region
    )
    print('✓ Azure AI Translator configured\n')
    
    # Detect language
    detected_lang = detect_language(transcribed_text, language_client)
    
    # Translate text
    if detected_lang:
        translations = translate_text(transcribed_text, translator_client, detected_lang)
    
    print("\n" + "="*60)
    print("PROCESSING COMPLETE")
    print("="*60)
else:
    print("\n✗ Transcription failed.")
```

### Step 3.2: Test Language Detection and Translation

Run your updated application:

```bash
python speech-analysis.py hotel-review.wav
```

You should now see:
1. Audio processing
2. Speech transcription
3. Language detection (English)
4. Translations to French, Spanish, German, and Hindi

---

## Part 4: Add Comprehensive Text Analysis

Now you'll add sentiment analysis, entity recognition, and key phrase extraction.

### Step 4.1: Add Text Analysis Functions

Add these functions to `speech-analysis.py` (before the `main()` function):

```python
def analyze_sentiment(text, language_client):
    """Analyze the sentiment of the text."""
    try:
        sentiment_analysis = language_client.analyze_sentiment(documents=[text])[0]
        
        print(f"Overall Sentiment: {sentiment_analysis.sentiment}")
        print(f"  Positive: {sentiment_analysis.confidence_scores.positive:.2%}")
        print(f"  Neutral: {sentiment_analysis.confidence_scores.neutral:.2%}")
        print(f"  Negative: {sentiment_analysis.confidence_scores.negative:.2%}")
        
    except Exception as e:
        print(f"✗ Error analyzing sentiment: {str(e)}")


def recognize_entities(text, language_client):
    """Recognize named entities in the text."""
    try:
        entities = language_client.recognize_entities(documents=[text])[0].entities
        
        if len(entities) > 0:
            print(f"Found {len(entities)} entities:")
            for entity in entities:
                print(f"  • {entity.text} ({entity.category})")
                if entity.subcategory:
                    print(f"    Subcategory: {entity.subcategory}")
                print(f"    Confidence: {entity.confidence_score:.2%}")
        else:
            print("No entities found.")
            
    except Exception as e:
        print(f"✗ Error recognizing entities: {str(e)}")


def extract_key_phrases(text, language_client):
    """Extract key phrases from the text."""
    try:
        key_phrases = language_client.extract_key_phrases(documents=[text])[0].key_phrases
        
        if len(key_phrases) > 0:
            print(f"Found {len(key_phrases)} key phrases:")
            for phrase in key_phrases:
                print(f"  • {phrase}")
        else:
            print("No key phrases found.")
            
    except Exception as e:
        print(f"✗ Error extracting key phrases: {str(e)}")


def recognize_linked_entities(text, language_client):
    """Recognize entities with links to external data sources."""
    try:
        linked_entities = language_client.recognize_linked_entities(documents=[text])[0].entities
        
        if len(linked_entities) > 0:
            print(f"Found {len(linked_entities)} linked entities:")
            for entity in linked_entities:
                print(f"  • {entity.name}")
                print(f"    Data source: {entity.data_source}")
                print(f"    URL: {entity.url}")
        else:
            print("No linked entities found.")
            
    except Exception as e:
        print(f"✗ Error recognizing linked entities: {str(e)}")


def analyze_text(text, language_client):
    """Perform comprehensive text analysis."""
    print("\n" + "-"*60)
    print("TEXT ANALYSIS")
    print("-"*60)
    
    print("\n--- Sentiment Analysis ---")
    analyze_sentiment(text, language_client)
    
    print("\n--- Entity Recognition ---")
    recognize_entities(text, language_client)
    
    print("\n--- Key Phrase Extraction ---")
    extract_key_phrases(text, language_client)
    
    print("\n--- Linked Entity Recognition ---")
    recognize_linked_entities(text, language_client)
```

### Step 4.2: Update Main Function

In the `main()` function, add text analysis after translation. Find this code block:

```python
# Translate text
if detected_lang:
    translations = translate_text(transcribed_text, translator_client, detected_lang)
```

Update it to:

```python
# Translate text and analyze
if detected_lang:
    translations = translate_text(transcribed_text, translator_client, detected_lang)
    analyze_text(transcribed_text, language_client)
```

### Step 4.3: Test Text Analysis

Run your application:

```bash
python speech-analysis.py hotel-review.wav
```

You should now see:
1. Audio processing and transcription
2. Language detection
3. Translation
4. Sentiment analysis
5. Entity recognition
6. Key phrases
7. Linked entities

---

## Part 5: Add Summarization and Function Calling

Final part: add Azure AI Language extractive text summarization and a special time command feature that demonstrates function calling.

### Step 5.1: Add Summarization and Time Functions

Add `import datetime` at the top of the file with other imports.

Add these functions to `speech-analysis.py` (before `main()`):

```python
def generate_summary(text, language_client):
    """Generate an extractive summary using Azure AI Language service."""
    try:
        print("\n" + "-"*60)
        print("TEXT SUMMARIZATION")
        print("-"*60)
        
        word_count = len(text.split())
        print(f"Original text: {word_count} words")
        
        # Azure requires at least a certain length for summarization
        if word_count < 20:
            print("\nNote: Text is quite short. Azure summarization works best with longer text.")
            print(f"Text: {text}")
            return
        
        # Import required for extractive summarization
        from azure.ai.textanalytics import ExtractiveSummaryAction
        
        # Determine number of sentences to extract (max 3 for shorter text)
        num_sentences = min(3, max(1, word_count // 20))
        
        print(f"Extracting top {num_sentences} sentence(s)...")
        
        # Create the summarization action
        poller = language_client.begin_analyze_actions(
            documents=[text],
            actions=[
                ExtractiveSummaryAction(max_sentence_count=num_sentences)
            ],
        )
        
        # Wait for the operation to complete
        document_results = poller.result()
        
        # Process results
        for result in document_results:
            extract_summary_result = result[0]  # first document, first result
            
            if extract_summary_result.is_error:
                print(f"✗ Error: {extract_summary_result.code} - {extract_summary_result.message}")
                return
            
            # Extract and display summary sentences
            summary_sentences = extract_summary_result.sentences
            
            if len(summary_sentences) > 0:
                print(f"\n✓ Extracted {len(summary_sentences)} key sentence(s):")
                print("-" * 60)
                
                # Sort by rank score (highest first) or by offset (document order)
                sorted_sentences = sorted(summary_sentences, 
                                        key=lambda s: s.rank_score, 
                                        reverse=True)
                
                for i, sentence in enumerate(sorted_sentences, 1):
                    print(f"\n{i}. {sentence.text}")
                    print(f"   Rank Score: {sentence.rank_score:.3f}")
                
                # Create combined summary
                print("\n" + "-" * 60)
                print("Combined Summary:")
                print("-" * 60)
                summary_text = " ".join([s.text for s in sorted_sentences])
                print(f"\n{summary_text}\n")
                
                summary_word_count = len(summary_text.split())
                compression = (1 - summary_word_count / word_count) * 100
                print(f"Summary length: {summary_word_count} words")
                print(f"Compression: {compression:.1f}%")
            else:
                print("\n✗ No summary sentences extracted.")
        
    except Exception as e:
        print(f"✗ Error generating summary: {str(e)}")
        import traceback
        traceback.print_exc()


def check_time_command(text):
    """Check if the text contains a time-related command."""
    time_keywords = [
        'what time is it',
        'tell me the time',
        'what is the time',
        'current time',
        "what's the time",
        'whats the time'
    ]
    
    text_lower = text.lower()
    for keyword in time_keywords:
        if keyword in text_lower:
            return True
    return False


def handle_time_command(speech_key, speech_region):
    """Handle the time command by getting system time and generating speech output."""
    try:
        print("\n" + "="*60)
        print("SPECIAL COMMAND DETECTED: Time Request")
        print("="*60)
        
        now = datetime.datetime.now()
        time_string = now.strftime("%I:%M %p")
        
        print(f"\nCurrent time: {time_string}")
        
        response_text = f"The current time is {time_string}."
        
        speech_config = speech_sdk.SpeechConfig(speech_key, speech_region)
        speech_config.speech_synthesis_voice_name = "en-US-JennyNeural"
        
        output_file = f"time_response_{now.strftime('%Y%m%d_%H%M%S')}.wav"
        audio_config = speech_sdk.audio.AudioOutputConfig(filename=output_file)
        
        speech_synthesizer = speech_sdk.SpeechSynthesizer(
            speech_config=speech_config,
            audio_config=audio_config
        )
        
        print(f"\nGenerating speech response: '{response_text}'")
        
        result = speech_synthesizer.speak_text_async(response_text).get()
        
        if result.reason == speech_sdk.ResultReason.SynthesizingAudioCompleted:
            print(f"✓ Speech synthesized successfully!")
            print(f"✓ Output saved to: {output_file}")
            print("\nYou can play this file to hear the response.")
        elif result.reason == speech_sdk.ResultReason.Canceled:
            cancellation = result.cancellation_details
            print(f"✗ Speech synthesis canceled: {cancellation.reason}")
            if cancellation.reason == speech_sdk.CancellationReason.Error:
                print(f"   Error details: {cancellation.error_details}")
                
    except Exception as e:
        print(f"✗ Error handling time command: {str(e)}")
```

### Step 5.2: Update Main Function

In the `main()` function, find this section in your code:

```python
# Inside main(), after transcribe_speech() call
if transcribed_text:
    print("\n" + "="*60)
    print("TRANSCRIPTION RESULT")
    print("="*60)
    print(f"\n{transcribed_text}\n")
    
    # Configure Language and Translator services
    # ... existing configuration code ...
    
    # Detect language
    detected_lang = detect_language(transcribed_text, language_client)
    
    # Translate text and analyze
    if detected_lang:
        translations = translate_text(transcribed_text, translator_client, detected_lang)
        analyze_text(transcribed_text, language_client)
    
    print("\n" + "="*60)
    print("PROCESSING COMPLETE")
    print("="*60)
else:
    print("\n✗ Transcription failed.")
```

Replace the entire `if transcribed_text:` block with:

```python
if transcribed_text:
    print("\n" + "="*60)
    print("TRANSCRIPTION RESULT")
    print("="*60)
    print(f"\n{transcribed_text}\n")
    
    # Check for special commands first
    if check_time_command(transcribed_text):
        handle_time_command(ai_endpoint, ai_region)
    else:
        # Configure Language and Translator services
        language_key = os.getenv('LANGUAGE_KEY')
        language_endpoint = os.getenv('LANGUAGE_ENDPOINT')
        language_credential = AzureKeyCredential(language_key)
        language_client = TextAnalyticsClient(
            endpoint=language_endpoint,
            credential=language_credential
        )
        print('✓ Azure AI Language configured')
        
        translator_key = os.getenv('TRANSLATOR_KEY')
        translator_region = os.getenv('TRANSLATOR_REGION')
        translator_credential = AzureKeyCredential(translator_key)
        translator_client = TextTranslationClient(
            credential=translator_credential,
            region=translator_region
        )
        print('✓ Azure AI Translator configured\n')
        
        # Detect language
        detected_lang = detect_language(transcribed_text, language_client)
        
        # Translate, analyze, and summarize
        if detected_lang:
            translations = translate_text(transcribed_text, translator_client, detected_lang)
            analyze_text(transcribed_text, language_client)
            generate_summary(transcribed_text, language_client)
        
        print("\n" + "="*60)
        print("PROCESSING COMPLETE")
        print("="*60)
else:
    print("\n✗ Transcription failed.")
```

### Step 5.3: Final Testing

Test with the time command:

```bash
python speech-analysis.py time-request.wav
```

You should see:
1. Audio processing
2. Transcription
3. Time command detection
4. Current time displayed
5. Speech response file generated

Test with the full analysis:

```bash
python speech-analysis.py hotel-review.wav
```

You should see the complete pipeline:
1. Audio processing and transcription
2. Language detection
3. Translation to 4 languages
4. Comprehensive text analysis
5. Text summarization

---

## Troubleshooting

### Common Issues

**1. FFmpeg not found**
```
Error: ffmpeg not found
```
**Solution**: Install ffmpeg:
- Windows: `choco install ffmpeg` or download from ffmpeg.org
- macOS: `brew install ffmpeg`
- Linux: `sudo apt-get install ffmpeg`

**2. Module not found errors**
```
ModuleNotFoundError: No module named 'azure'
```
**Solution**: Ensure virtual environment is activated and packages are installed:
```bash
# Activate venv
.\venv\Scripts\Activate.ps1  # Windows
source venv/bin/activate  # macOS/Linux

# Install packages
pip install -r requirements.txt
```

**3. Authentication errors**
```
Error: Invalid subscription key
```
**Solution**: Check your `.env` file has correct keys and regions from Azure Portal.

**4. Audio file errors**
```
Error reading audio file
```
**Solution**: Ensure the audio file exists and is a valid WAV file.

**5. Rate limit exceeded**
```
Error: Too many requests
```
**Solution**: You're using the free tier. Wait a minute and try again, or use fewer features.

---

## Clean Up Resources

When you're finished with the lab:

1. Stop your application
2. Deactivate your virtual environment: `deactivate`
3. In Azure Portal, navigate to your resource group `speech-language-lab-rg`
4. Click **Delete resource group**
5. Type the resource group name to confirm
6. Click **Delete**

This removes all Azure resources and stops any charges.

---

## Additional Resources

- [Azure AI Speech documentation](https://learn.microsoft.com/azure/ai-services/speech-service/)
- [Azure AI Translator documentation](https://learn.microsoft.com/azure/ai-services/translator/)
- [Azure AI Language documentation](https://learn.microsoft.com/azure/ai-services/language-service/)
- [Python Speech SDK samples](https://github.com/Azure-Samples/cognitive-services-speech-sdk)
