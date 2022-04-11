using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

// Import namespaces
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace OCRDemo;

class Program
{
  private static ComputerVisionClient cvClient;
  static async Task Main(string[] args)
  {
    try
    {
      // Get config settings from AppSettings
      IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
      IConfigurationRoot configuration = builder.Build();
      string cogSvcEndpoint = configuration["CognitiveServicesEndpoint"];
      string cogSvcKey = configuration["CognitiveServiceKey"];

      // Authenticate Computer Vision client
      ApiKeyServiceClientCredentials credentials = new ApiKeyServiceClientCredentials(cogSvcKey);
      cvClient = new ComputerVisionClient(credentials)
      {
        Endpoint = cogSvcEndpoint
      };

      // Menu for text reading functions
      Console.WriteLine("1: Use OCR API\n2: Use Read API\n3: Analyze Image\n4: DP-900 Demo\nAny other key to quit");
      Console.WriteLine("Enter a number:");
      string command = Console.ReadLine();
      string imageFile;
      switch (command)
      {
        case "1":
          imageFile = "images/Lincoln.jpg";
          await GetTextOcr(imageFile);
          break;
        case "2":
          imageFile = "images/Rome.pdf";
          await GetTextRead(imageFile);
          break;
        case "3":
          imageFile = "images/berg.jpg"; //avengers //dracula //berg
          await GetTextAnalysis(imageFile);
          break;
        case "4":
          imageFile = "images/Notiz.png";
          await GetTextRead(imageFile);
          break;
        default:
          break;
      }

    }
    catch (Exception ex)
    {
      Console.WriteLine(ex.Message);
    }
  }

  static async Task GetTextOcr(string imageFile)
  {
    Console.WriteLine($"Reading text in {imageFile}\n");

    // Use OCR API to read text in image
    using (var imageData = File.OpenRead(imageFile))
    {
      var ocrResults = await cvClient.RecognizePrintedTextInStreamAsync(detectOrientation: false, image: imageData);

      // Prepare image for drawing
      Image image = Image.FromFile(imageFile);
      Graphics graphics = Graphics.FromImage(image);
      Pen pen = new Pen(Color.Magenta, 3);

      foreach (var region in ocrResults.Regions)
      {
        foreach (var line in region.Lines)
        {
          // Show the position of the line of text
          int[] dims = line.BoundingBox.Split(",").Select(int.Parse).ToArray();
          Rectangle rect = new Rectangle(dims[0], dims[1], dims[2], dims[3]);
          graphics.DrawRectangle(pen, rect);

          // Read the words in the line of text
          string lineText = "";
          foreach (var word in line.Words)
          {
            lineText += word.Text + " ";
          }

          Console.WriteLine(lineText.Trim());
        }
      }

      // Save the image with the text locations highlighted
      String output_file = "ocr_results.jpg";
      image.Save(output_file);
      Console.WriteLine("Results saved in " + output_file);
    }
  }

  static async Task GetTextRead(string imageFile)
  {
    Console.WriteLine($"Reading text in {imageFile}\n");

    // Use Read API to read text in image
    using (var imageData = File.OpenRead(imageFile))
    {
      var readOp = await cvClient.ReadInStreamAsync(imageData);

      // Get the async operation ID so we can check for the results
      string operationLocation = readOp.OperationLocation;
      string operationId = operationLocation.Substring(operationLocation.Length - 36);

      // Wait for the asynchronous operation to complete
      ReadOperationResult results;

      do
      {
        Thread.Sleep(1000);
        results = await cvClient.GetReadResultAsync(Guid.Parse(operationId));
      }
      while ((results.Status == OperationStatusCodes.Running || results.Status == OperationStatusCodes.NotStarted));

      // If the operation was successfuly, process the text line by line
      if (results.Status == OperationStatusCodes.Succeeded)
      {
        var textUrlFileResults = results.AnalyzeResult.ReadResults;

        foreach (ReadResult page in textUrlFileResults)
        {
          foreach (Line line in page.Lines)
          {
            Console.WriteLine(line.Text);
          }
        }
      }
    }
  }

  static async Task GetTextAnalysis(string imageFile)
  {
    Console.WriteLine($"Analyzing Image: {imageFile}\n");

    // Specify features to be retrieved  
    List<VisualFeatureTypes?> features = new List<VisualFeatureTypes?>()
    {
        VisualFeatureTypes.Description,
        VisualFeatureTypes.Tags,
        VisualFeatureTypes.Categories,
        VisualFeatureTypes.Brands,
        VisualFeatureTypes.Objects,
        VisualFeatureTypes.Adult
    };

    // Get image analysis
    using (var imageData = File.OpenRead(imageFile))
    {
      var analysis = await cvClient.AnalyzeImageInStreamAsync(imageData, features);

      // get image captions
      foreach (var caption in analysis.Description.Captions)
      {
        Console.WriteLine($"Description: {caption.Text} (confidence: {caption.Confidence.ToString("P")})");
      }

      // Get image tags
      if (analysis.Tags.Count > 0)
      {
        Console.WriteLine("Tags:");
        foreach (var tag in analysis.Tags)
        {
          Console.WriteLine($" -{tag.Name} (confidence: {tag.Confidence.ToString("P")})");
        }
      }

      // Get image categories (including celebrities and landmarks)
      List<LandmarksModel> landmarks = new List<LandmarksModel> { };
      List<CelebritiesModel> celebrities = new List<CelebritiesModel> { };
      Console.WriteLine("Categories:");
      foreach (var category in analysis.Categories)
      {
        // Print the category
        Console.WriteLine($" -{category.Name} (confidence: {category.Score.ToString("P")})");

        // Get landmarks in this category
        if (category.Detail?.Landmarks != null)
        {
          foreach (LandmarksModel landmark in category.Detail.Landmarks)
          {
            if (!landmarks.Any(item => item.Name == landmark.Name))
            {
              landmarks.Add(landmark);
            }
          }
        }

        // Get celebrities in this category
        if (category.Detail?.Celebrities != null)
        {
          foreach (CelebritiesModel celebrity in category.Detail.Celebrities)
          {
            if (!celebrities.Any(item => item.Name == celebrity.Name))
            {
              celebrities.Add(celebrity);
            }
          }
        }
      }

      // If there were landmarks, list them
      if (landmarks.Count > 0)
      {
        Console.WriteLine("Landmarks:");
        foreach (LandmarksModel landmark in landmarks)
        {
          Console.WriteLine($" -{landmark.Name} (confidence: {landmark.Confidence.ToString("P")})");
        }
      }

      // If there were celebrities, list them
      if (celebrities.Count > 0)
      {
        Console.WriteLine("Celebrities:");
        foreach (CelebritiesModel celebrity in celebrities)
        {
          Console.WriteLine($" -{celebrity.Name} (confidence: {celebrity.Confidence.ToString("P")})");
        }
      }

      // Get brands in the image
      if (analysis.Brands.Count > 0)
      {
        Console.WriteLine("Brands:");
        foreach (var brand in analysis.Brands)
        {
          Console.WriteLine($" -{brand.Name} (confidence: {brand.Confidence.ToString("P")})");
        }
      }

      // Get objects in the image
      if (analysis.Objects.Count > 0)
      {
        Console.WriteLine("Objects in image:");

        // Prepare image for drawing
        Image image = Image.FromFile(imageFile);
        Graphics graphics = Graphics.FromImage(image);
        Pen pen = new Pen(Color.Cyan, 3);
        Font font = new Font("Arial", 16);
        SolidBrush brush = new SolidBrush(Color.Black);

        foreach (var detectedObject in analysis.Objects)
        {
          // Print object name
          Console.WriteLine($" -{detectedObject.ObjectProperty} (confidence: {detectedObject.Confidence.ToString("P")})");

          // Draw object bounding box
          var r = detectedObject.Rectangle;
          Rectangle rect = new Rectangle(r.X, r.Y, r.W, r.H);
          graphics.DrawRectangle(pen, rect);
          graphics.DrawString(detectedObject.ObjectProperty, font, brush, r.X, r.Y);

        }
        // Save annotated image
        String output_file = "objects.jpg";
        image.Save(output_file);
        Console.WriteLine("  Results saved in " + output_file);
      }

      // Get moderation ratings
      string ratings = $"Ratings:\n -Adult: {analysis.Adult.IsAdultContent}\n -Racy: {analysis.Adult.IsRacyContent}\n -Gore: {analysis.Adult.IsGoryContent}";
      Console.WriteLine(ratings);
    }
  }
}
