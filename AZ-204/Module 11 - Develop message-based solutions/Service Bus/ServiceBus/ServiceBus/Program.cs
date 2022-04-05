using System;
using Azure.Messaging.ServiceBus;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.IO;

namespace ServiceBus
{
  class Program
  {
    static JObject azureConfig = JObject.Parse(File.ReadAllText("azureConfig.json"));

    // Batch Account credentials
    static String ServiceBusConnectionString = azureConfig.GetValue("ServiceBusConnectionString").ToString();
    const string QueueName = "q";
    const string TopicName = "mytopic";

    static ServiceBusClient serviceBusClient;
    static ServiceBusSender queueSender;
    static ServiceBusSender topicSender;

    static void Main(string[] args)
    {
      MainAsync().GetAwaiter().GetResult();
    }

    static async Task MainAsync()
    {
      const int numberOfMessages = 10;

      //In Case of a proxy - use Port 443
      /*
      var options = new ServiceBusClientOptions
      {
          TransportType = ServiceBusTransportType.AmqpWebSockets
      };
      */

      serviceBusClient = new ServiceBusClient(ServiceBusConnectionString/* , options */);
      
      queueSender = serviceBusClient.CreateSender(QueueName);
      //topicSender = serviceBusClient.CreateSender(TopicName);

      Console.WriteLine("======================================================");
      Console.WriteLine("Press ENTER key to exit after sending all the messages.");
      Console.WriteLine("======================================================");

      // Send messages.
      //await SendMessagesAsync(numberOfMessages);
      while (Console.ReadLine() != "quit")
      {
        await SendMessagesAsync(numberOfMessages);
      }

      await serviceBusClient.DisposeAsync();
    }

    static async Task SendMessagesAsync(int numberOfMessagesToSend)
    {
      try
      {
        for (var i = 0; i < numberOfMessagesToSend; i++)
        {
          // Create a new message 
          string messageBody = $"Message {i}";

          ServiceBusMessage topicMessage = new ServiceBusMessage(Encoding.UTF8.GetBytes(messageBody + " T"));
          ServiceBusMessage queueMessage = new ServiceBusMessage(Encoding.UTF8.GetBytes(messageBody + " Q"));
          queueMessage.Subject = "myLabel";
          //queueMessage.PartitionKey = "1"; // Define the partition to sent to

          // Write the body of the message to the console.
          Console.WriteLine($"Sending messages: {messageBody}");

          // Send the message to the queue and topic.
        //  await topicSender.SendMessageAsync(topicMessage);
          await queueSender.SendMessageAsync(queueMessage);
        }
      }
      catch (Exception exception)
      {
        Console.WriteLine($"{DateTime.Now} :: Exception: {exception.Message}");
      }
    }
  }
}
