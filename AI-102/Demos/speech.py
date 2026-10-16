"""
Multi-Language Translation and Speech Processing
This application demonstrates:
1. Text translation (any language -> English)
2. Text-to-Speech synthesis (English text -> audio)
3. Speech translation (English audio -> German text)
"""

import os
from dotenv import load_dotenv
from azure.core.credentials import AzureKeyCredential
from azure.ai.translation.text import TextTranslationClient
from azure.ai.translation.text.models import InputTextItem
import azure.cognitiveservices.speech as speech_sdk

def main():
    try:
        # Load configuration
        load_dotenv()
        
        # Get Azure AI Translator credentials
        translator_key = os.getenv('TRANSLATOR_KEY')
        translator_region = os.getenv('TRANSLATOR_REGION')
        
        # Get Azure AI Speech credentials
        speech_key = os.getenv('SPEECH_KEY')
        speech_region = os.getenv('SPEECH_REGION')
        
        # Validate configuration
        if not all([translator_key, translator_region, speech_key, speech_region]):
            print("ERROR: Please configure all required credentials in .env file")
            print("Required: TRANSLATOR_KEY, TRANSLATOR_REGION, SPEECH_KEY, SPEECH_REGION")
            return
        
        # Create Text Translation client
        translator_credential = AzureKeyCredential(translator_key)
        translator_client = TextTranslationClient(
            credential=translator_credential, 
            region=translator_region
        )
        
        # Configure Speech services
        speech_config = speech_sdk.SpeechConfig(speech_key, speech_region)
        
        print("=" * 60)
        print("Multi-Language Translation and Speech Processing")
        print("=" * 60)
        print("\nServices initialized successfully!")
        print(f"Text Translation Region: {translator_region}")
        print(f"Speech Service Region: {speech_region}")
        print("\n" + "=" * 60)
        
        # Main processing loop
        while True:
            print("\nEnter text in any language (or 'quit' to exit):")
            input_text = input("> ").strip()
            
            if input_text.lower() in ['quit', 'exit', 'q']:
                print("\nThank you for using the translation service!")
                break
            
            if not input_text:
                print("Please enter some text.")
                continue
            
            # Step 1: Translate text to English
            print("\n[Step 1] Translating to English...")
            english_text = translate_to_english(translator_client, input_text)
            
            if not english_text:
                print("Translation failed. Please try again.")
                continue
            
            # Step 2: Synthesize English speech
            print("\n[Step 2] Synthesizing English speech...")
            audio_file = "english_speech.wav"
            if not synthesize_speech(speech_config, english_text, audio_file):
                print("Speech synthesis failed. Please try again.")
                continue
            
            # Step 3: Translate speech from English to German
            print("\n[Step 3] Translating speech to German...")
            german_text = translate_speech_to_german(speech_key, speech_region, audio_file)
            
            if german_text:
                print("\n" + "=" * 60)
                print("FINAL RESULT:")
                print(f"Original: {input_text}")
                print(f"English:  {english_text}")
                print(f"German:   {german_text}")
                print("=" * 60)
            else:
                print("Speech translation failed. Please try again.")
            
            # Clean up audio file
            # if os.path.exists(audio_file):
            #     try:
            #         os.remove(audio_file)
            #     except:
            #         pass
    
    except Exception as e:
        print(f"\nAn error occurred: {str(e)}")
        print("Please check your configuration and try again.")


def translate_to_english(client, text):
    """
    Translates input text to English using Azure AI Translator
    Auto-detects the source language
    """
    try:
        input_text_elements = [InputTextItem(text=text)]
        response = client.translate(
            body=input_text_elements, 
            to_language=['en']
        )
        
        if response and len(response) > 0:
            translation = response[0]
            detected_language = translation.detected_language.language
            detected_confidence = translation.detected_language.score
            
            print(f"   Detected language: {detected_language} (confidence: {detected_confidence:.2f})")
            
            if translation.translations:
                english_text = translation.translations[0].text
                print(f"   English translation: {english_text}")
                return english_text
        
        return None
    
    except Exception as e:
        print(f"   Translation error: {str(e)}")
        return None


def synthesize_speech(speech_config, text, output_file):
    """
    Converts English text to speech and saves to WAV file
    Uses a natural English voice
    """
    try:
        # Configure English voice (US English - female)
        speech_config.speech_synthesis_voice_name = "en-US-JennyNeural"
        
        # Configure audio output to file
        audio_config = speech_sdk.audio.AudioOutputConfig(filename=output_file)
        
        # Create speech synthesizer
        speech_synthesizer = speech_sdk.SpeechSynthesizer(speech_config, audio_config)
        
        # Synthesize speech
        print(f"   Synthesizing: '{text}'")
        result = speech_synthesizer.speak_text_async(text).get()
        
        if result.reason == speech_sdk.ResultReason.SynthesizingAudioCompleted:
            print(f"   Speech saved to: {output_file}")
            return True
        else:
            print(f"   Synthesis failed: {result.reason}")
            if result.reason == speech_sdk.ResultReason.Canceled:
                cancellation = result.cancellation_details
                print(f"   Error: {cancellation.error_details}")
            return False
    
    except Exception as e:
        print(f"   Speech synthesis error: {str(e)}")
        return False


def translate_speech_to_german(speech_key, speech_region, audio_file):
    """
    Translates English speech to German text using Speech Translation API
    Reads from the synthesized audio file
    """
    try:
        # Configure speech translation
        translation_config = speech_sdk.translation.SpeechTranslationConfig(
            speech_key, 
            speech_region
        )
        
        # Set source language (English) and target language (German)
        translation_config.speech_recognition_language = 'en-US'
        translation_config.add_target_language('de')
        
        # Configure audio input from file
        audio_config = speech_sdk.AudioConfig(filename=audio_file)
        
        # Create translation recognizer
        translator = speech_sdk.translation.TranslationRecognizer(
            translation_config, 
            audio_config=audio_config
        )
        
        print(f"   Processing audio file: {audio_file}")
        
        # Recognize and translate
        result = translator.recognize_once_async().get()
        
        if result.reason == speech_sdk.ResultReason.TranslatedSpeech:
            print(f"   Recognized English: {result.text}")
            
            if 'de' in result.translations:
                german_translation = result.translations['de']
                print(f"   German translation: {german_translation}")
                return german_translation
            else:
                print("   German translation not found in result")
                return None
        
        elif result.reason == speech_sdk.ResultReason.NoMatch:
            print("   No speech could be recognized from the audio")
            return None
        
        elif result.reason == speech_sdk.ResultReason.Canceled:
            cancellation = result.cancellation_details
            print(f"   Translation canceled: {cancellation.reason}")
            if cancellation.reason == speech_sdk.CancellationReason.Error:
                print(f"   Error: {cancellation.error_details}")
            return None
        
        return None
    
    except Exception as e:
        print(f"   Speech translation error: {str(e)}")
        return None


if __name__ == '__main__':
    main()
