https://gist.github.com/adntobias/2438d5987b546f9644fa83cb0954d858

---
lab:
    title: 'Analyze images and read text with Azure AI Vision'
    description: 'Use Azure AI Vision Image Analysis to analyze images, detect objects and people, and use OCR to extract text from images.'
---

# Analyze images and read text with Azure AI Vision

Azure AI Vision is an artificial intelligence capability that enables software systems to interpret visual input by analyzing images. In Microsoft Azure, the **Vision** Azure AI service provides pre-built models for common computer vision tasks, including analysis of images to suggest captions and tags, detection of common objects and people, removal of backgrounds, and optical character recognition (OCR) to extract text from images and documents.

This exercise takes approximately **60** minutes.

## Provision an Azure AI Vision resource

If you don't already have one in your subscription, you'll need to provision an Azure AI Vision resource.

> **Note**: In this exercise, you'll use a standalone **Computer Vision** resource. You can also use Azure AI Vision services in an *Azure AI Services* multi-service resource, either directly or in an *Azure AI Foundry* project.

1. Open the [Azure portal](https://portal.azure.com) at `https://portal.azure.com`, and sign in using your Azure credentials. Close any welcome messages or tips that are displayed.
1. Select **Create a resource**.
1. In the search bar, search for `Computer Vision`, select **Computer Vision**, and create the resource with the following settings:
    - **Subscription**: *Your Azure subscription*
    - **Resource group**: *Create or select a resource group*
    - **Region**: *Choose from **East US**, **West US**, **France Central**, **Korea Central**, **North Europe**, **Southeast Asia**, **West Europe**, or **East Asia**\**
    - **Name**: *A valid name for your Computer Vision resource*
    - **Pricing tier**: Free F0

    \*Azure AI Vision 4.0 full feature sets are currently only available in these regions.

1. Select the required checkboxes and create the resource.
1. Wait for deployment to complete, and then view the deployment details.
1. When the resource has been deployed, go to it and under the **Resource management** node in the navigation pane, view its **Keys and Endpoint** page. You will need the endpoint and one of the keys from this page in the next procedure.

## Part 1: Analyze images with the Azure AI Vision SDK

In this part of the exercise, you'll develop a Python client application that uses the Azure AI Vision SDK to analyze images, generate captions, detect objects, and identify people.

### Prepare the development environment

1. Create a project folder for this exercise and navigate to it:

    ```powershell
    mkdir vision-lab
    cd vision-lab
    ```

1. Create a Python virtual environment:

    ```powershell
    python -m venv venv
    .\venv\Scripts\Activate.ps1
    ```

    > **Note**: On macOS/Linux, use `source venv/bin/activate` instead.

1. Install the required packages:
    
    ```powershell
    pip install python-dotenv azure-ai-vision-imageanalysis==1.0.0 Pillow matplotlib
    ```

1. Create a configuration file named `.env` using your preferred text editor and add the following content:

    ```
    AI_SERVICE_ENDPOINT=your_endpoint_here
    AI_SERVICE_KEY=your_key_here
    ```

1. Update the `.env` file with the **endpoint** and an authentication **key** for your Computer Vision resource (copied from its **Keys and Endpoint** page in the Azure portal). Save your changes.

1. Create a subfolder named `images` to store test images:

    ```powershell
    mkdir images
    ```

1. Download sample images to test with. You can use your own images or download some from the web. Save at least three images (e.g., `street.jpg`, `building.jpg`, `person.jpg`) in the `images` folder.

### Create the image analysis application

Now you'll create a Python application to analyze images.

1. Create a new file named `image-analysis.py` in your code editor.

1. Add the following code to import the required namespaces and set up the main function:

    ```python
    from dotenv import load_dotenv
    import os
    from PIL import Image, ImageDraw
    import sys
    from matplotlib import pyplot as plt
    
    # import namespaces
    from azure.ai.vision.imageanalysis import ImageAnalysisClient
    from azure.ai.vision.imageanalysis.models import VisualFeatures
    from azure.core.credentials import AzureKeyCredential

    def main():
        # Clear the console
        os.system('cls' if os.name=='nt' else 'clear')

        try:
            # Get Configuration Settings
            load_dotenv()
            ai_endpoint = os.getenv('AI_SERVICE_ENDPOINT')
            ai_key = os.getenv('AI_SERVICE_KEY')

            # Get image
            image_file = 'images/street.jpg'
            if len(sys.argv) > 1:
                image_file = sys.argv[1]
            
            # Authenticate Azure AI Vision client
            cv_client = ImageAnalysisClient(
                endpoint=ai_endpoint,
                credential=AzureKeyCredential(ai_key)
            )

            # Analyze image
            with open(image_file, "rb") as f:
                image_data = f.read()
            print(f'\nAnalyzing {image_file}\n')

            result = cv_client.analyze(
                image_data=image_data,
                visual_features=[
                    VisualFeatures.CAPTION,
                    VisualFeatures.DENSE_CAPTIONS,
                    VisualFeatures.TAGS,
                    VisualFeatures.OBJECTS,
                    VisualFeatures.PEOPLE],
            )
                
            # Get image captions
            if result.caption is not None:
                print("\nCaption:")
                print(" Caption: '{}' (confidence: {:.2f}%)".format(result.caption.text, result.caption.confidence * 100))
                
            if result.dense_captions is not None:
                print("\nDense Captions:")
                for caption in result.dense_captions.list:
                    print(" Caption: '{}' (confidence: {:.2f}%)".format(caption.text, caption.confidence * 100))

            # Get image tags
            if result.tags is not None:
                print("\nTags:")
                for tag in result.tags.list:
                    print(" Tag: '{}' (confidence: {:.2f}%)".format(tag.name, tag.confidence * 100))

            # Get objects in the image
            if result.objects is not None:
                print("\nObjects in image:")
                for detected_object in result.objects.list:
                    # Print object tag and confidence
                    print(" {} (confidence: {:.2f}%)".format(detected_object.tags[0].name, detected_object.tags[0].confidence * 100))
                # Annotate objects in the image
                show_objects(image_file, result.objects.list)

            # Get people in the image
            if result.people is not None:
                print("\nPeople in image:")

                for detected_person in result.people.list:
                    if detected_person.confidence > 0.2:
                        # Print location and confidence of each person detected
                        print(" {} (confidence: {:.2f}%)".format(detected_person.bounding_box, detected_person.confidence * 100))
                # Annotate people in the image
                show_people(image_file, result.people.list)
                
        except Exception as ex:
            print(ex)


    def show_objects(image_filename, detected_objects):
        print("\nAnnotating objects...")

        # Prepare image for drawing
        image = Image.open(image_filename)
        fig = plt.figure(figsize=(image.width/100, image.height/100))
        plt.axis('off')
        draw = ImageDraw.Draw(image)
        color = 'cyan'

        for detected_object in detected_objects:
            # Draw object bounding box
            r = detected_object.bounding_box
            bounding_box = ((r.x, r.y), (r.x + r.width, r.y + r.height)) 
            draw.rectangle(bounding_box, outline=color, width=3)
            plt.annotate(detected_object.tags[0].name,(r.x, r.y), backgroundcolor=color)

        # Save annotated image
        plt.imshow(image)
        plt.tight_layout(pad=0)
        objectfile = 'objects.jpg'
        fig.savefig(objectfile)
        print('  Results saved in', objectfile)


    def show_people(image_filename, detected_people):
        print("\nAnnotating people...")

        # Prepare image for drawing
        image = Image.open(image_filename)
        fig = plt.figure(figsize=(image.width/100, image.height/100))
        plt.axis('off')
        draw = ImageDraw.Draw(image)
        color = 'cyan'

        for detected_person in detected_people:
            if detected_person.confidence > 0.2:
                # Draw object bounding box
                r = detected_person.bounding_box
                bounding_box = ((r.x, r.y), (r.x + r.width, r.y + r.height))
                draw.rectangle(bounding_box, outline=color, width=3)

        # Save annotated image
        plt.imshow(image)
        plt.tight_layout(pad=0)
        peoplefile = 'people.jpg'
        fig.savefig(peoplefile)
        print('  Results saved in', peoplefile)


    if __name__ == "__main__":
        main()
    ```

1. Save the file and test the application:

    ```powershell
    python image-analysis.py images/street.jpg
    ```

1. Observe the output, which should include captions, tags, detected objects, and detected people for the image.

1. Open the generated `objects.jpg` and `people.jpg` files to view the annotated images with bounding boxes.

1. Test the application with other images:

    ```powershell
    python image-analysis.py images/building.jpg
    python image-analysis.py images/person.jpg
    ```

## Part 2: Read text from images using OCR

Now that you've explored image analysis capabilities, you'll use the same Azure AI Vision resource to implement optical character recognition (OCR) to extract text from images and documents.

### Create the OCR application

1. Create a new file named `read-text.py` in your code editor.

1. Add the following complete code to create the OCR application:

    ```python
    from dotenv import load_dotenv
    import os
    import sys
    from PIL import Image, ImageDraw
    from matplotlib import pyplot as plt

    # import namespaces
    from azure.ai.vision.imageanalysis import ImageAnalysisClient
    from azure.ai.vision.imageanalysis.models import VisualFeatures
    from azure.core.credentials import AzureKeyCredential

    def main():
        # Clear the console
        os.system('cls' if os.name=='nt' else 'clear')

        try:
            # Get Configuration Settings
            load_dotenv()
            ai_endpoint = os.getenv('AI_SERVICE_ENDPOINT')
            ai_key = os.getenv('AI_SERVICE_KEY')

            # Get image
            image_file = 'images/Lincoln.jpg'
            if len(sys.argv) > 1:
                image_file = sys.argv[1]

            # Authenticate Azure AI Vision client
            cv_client = ImageAnalysisClient(
                endpoint=ai_endpoint,
                credential=AzureKeyCredential(ai_key)
            )
            
            # Read text in image
            with open(image_file, "rb") as f:
                image_data = f.read()
            print(f"\nReading text in {image_file}")

            result = cv_client.analyze(
                image_data=image_data,
                visual_features=[VisualFeatures.READ])

            # Print the text
            if result.read is not None:
                print("\nText:")
                
                for line in result.read.blocks[0].lines:
                    print(f" {line.text}")        
                # Annotate the text in the image
                annotate_lines(image_file, result.read)

                # Find individual words in each line
                print("\nIndividual words:")
                for line in result.read.blocks[0].lines:
                    for word in line.words:
                        print(f"  {word.text} (Confidence: {word.confidence:.2f})")
                # Annotate the words in the image
                annotate_words(image_file, result.read)

        except Exception as ex:
            print(ex)


    def annotate_lines(image_file, detected_text):
        print(f'\nAnnotating lines of text in image...')

        # Prepare image for drawing
        image = Image.open(image_file)
        fig = plt.figure(figsize=(image.width/100, image.height/100))
        plt.axis('off')
        draw = ImageDraw.Draw(image)
        color = 'cyan'

        for line in detected_text.blocks[0].lines:
            # Draw line bounding polygon
            r = line.bounding_polygon
            rectangle = ((r[0].x, r[0].y),(r[1].x, r[1].y),(r[2].x, r[2].y),(r[3].x, r[3].y))
            draw.polygon(rectangle, outline=color, width=3)

        # Save image
        plt.imshow(image)
        plt.tight_layout(pad=0)
        textfile = 'lines.jpg'
        fig.savefig(textfile)
        print('  Results saved in', textfile)
        

    def annotate_words(image_file, detected_text):
        print(f'\nAnnotating individual words in image...')

        # Prepare image for drawing
        image = Image.open(image_file)
        fig = plt.figure(figsize=(image.width/100, image.height/100))
        plt.axis('off')
        draw = ImageDraw.Draw(image)
        color = 'cyan'

        for line in detected_text.blocks[0].lines:
            for word in line.words:
                # Draw word bounding polygon
                r = word.bounding_polygon
                rectangle = ((r[0].x, r[0].y),(r[1].x, r[1].y),(r[2].x, r[2].y),(r[3].x, r[3].y))
                draw.polygon(rectangle, outline=color, width=3)

        # Save image
        plt.imshow(image)
        plt.tight_layout(pad=0)
        textfile = 'words.jpg'
        fig.savefig(textfile)
        print('  Results saved in', textfile)


    if __name__ == "__main__":
        main()
    ```

1. Save the file.

1. Download or create test images with text content. You'll need images containing text (such as photos of signs, documents, or handwritten notes). Save them in the `images` folder with names like `Lincoln.jpg`, `Business-card.jpg`, and `Note.jpg`.

1. Test the OCR application:

    ```powershell
    python read-text.py images/Lincoln.jpg
    ```

1. Observe the output, which should display:
   - All lines of text detected in the image
   - Individual words with confidence scores
   - Generated `lines.jpg` showing text lines highlighted
   - Generated `words.jpg` showing individual words highlighted

1. Open the generated `lines.jpg` and `words.jpg` files to view the annotated images.

1. Test with other images containing text:

    ```powershell
    python read-text.py images/Business-card.jpg
    python read-text.py images/Note.jpg
    ```

## Understanding the code

Let's review what the code does:

### Image Analysis Application

- **Configuration**: Uses `python-dotenv` to load Azure credentials from the `.env` file
- **Image Loading**: Opens and reads image files as binary data
- **Analysis**: Calls the Azure AI Vision API with multiple visual features:
  - `CAPTION`: Generates a descriptive caption for the entire image
  - `DENSE_CAPTIONS`: Provides multiple captions for different regions
  - `TAGS`: Identifies relevant tags/keywords
  - `OBJECTS`: Detects and locates objects with bounding boxes
  - `PEOPLE`: Detects and locates people with bounding boxes
- **Visualization**: Uses Pillow and Matplotlib to draw bounding boxes on images

### OCR Application

- **Configuration**: Uses the same Azure credentials
- **Text Reading**: Uses the `READ` visual feature to extract text
- **Line Detection**: Identifies complete lines of text with polygon boundaries
- **Word Detection**: Breaks down lines into individual words with confidence scores
- **Annotation**: Draws polygons around detected text for visualization

## Clean up resources

If you've finished exploring Azure AI Vision, you should delete the resources you have created in this exercise to avoid incurring unnecessary Azure costs:

1. Open the Azure portal at `https://portal.azure.com`, and sign in using the Microsoft account associated with your Azure subscription.

1. In the top search bar, search for *Computer Vision*, and select the Computer Vision resource you created in this lab.

1. On the resource page, select **Delete** and follow the instructions to delete the resource.

## Summary

In this lab, you've learned how to:

- Provision an Azure AI Vision resource
- Use the Azure AI Vision SDK to analyze images and generate captions
- Detect and tag objects and people in images
- Use optical character recognition (OCR) to extract text from images and documents
- Identify individual words and their locations in images
- Build complete Python applications from scratch using the Azure AI Vision SDK

You've successfully completed a comprehensive exploration of Azure AI Vision's image analysis and text extraction capabilities!
