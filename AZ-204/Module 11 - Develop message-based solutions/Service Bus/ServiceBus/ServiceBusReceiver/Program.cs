using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Newtonsoft.Json.Linq;
using System.IO;
namespace ServiceBusReceiver
{
    class Program
    {
        static JObject azureConfig = JObject.Parse(File.ReadAllText("azureConfig.json"));

        // Batch Account credentials
        static String ServiceBusConnectionString = azureConfig.GetValue("ServiceBusConnectionString").ToString();
        //static String ServiceBusConnectionString = "Endpoint=sb://servicebusnskr.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=U6ltMfbkcv+BLV3WaP83UQv8vtFE1pvNqTL+lVnmAds=";
        const string QueueName = "myqueue";
        const string TopicName = "mytopic";
        const string SubscriptionName = "mysubs";

        static ServiceBusClient serviceBusClient;
        static ServiceBusProcessor serviceBusProcessor;

        static void Main(string[] args)
        {
            MainAsync().GetAwaiter().GetResult();
        }

        static async Task MainAsync()
        {
            serviceBusClient = new ServiceBusClient(ServiceBusConnectionString);
            //serviceBusProcessor = serviceBusClient.CreateProcessor(TopicName, SubscriptionName, RegisterOptions());
            serviceBusProcessor = serviceBusClient.CreateProcessor(QueueName, RegisterOptions());
            serviceBusProcessor.ProcessErrorAsync += ExceptionReceivedHandler;
            serviceBusProcessor.ProcessMessageAsync += ProcessMessagesAsync;

            
        
            Console.WriteLine("======================================================");
            Console.WriteLine("Press ENTER key to exit after receiving all the messages.");
            Console.WriteLine("======================================================");

            // Register the queue message handler and receive messages in a loop
            try
            {
                serviceBusProcessor.StartProcessingAsync();
            }
            catch (ServiceBusException e)
            {
                System.Console.WriteLine("OH SNAP!: "+e.Message);
            }
            
            
            Console.ReadLine();
            await serviceBusProcessor.StopProcessingAsync();
            
            await serviceBusProcessor.CloseAsync();
            await serviceBusClient.DisposeAsync();
        }

        static ServiceBusProcessorOptions RegisterOptions()
        {
            // Configure the message handler options in terms of exception handling, number of concurrent messages to deliver, etc.
            var messageHandlerOptions = new ServiceBusProcessorOptions
            {
                // Maximum number of concurrent calls to the callback ProcessMessagesAsync(), set to 1 for simplicity.
                // Set it according to how many messages the application wants to process in parallel.
                MaxConcurrentCalls = 1,

                // Indicates whether the message pump should automatically complete the messages after returning from user callback.
                // False below indicates the complete operation is handled by the user callback as in ProcessMessagesAsync().
                AutoCompleteMessages = false, 
                //ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete
            };

            return messageHandlerOptions;
        }

        static async Task ProcessMessagesAsync(ProcessMessageEventArgs args)
        {
            // Process the message.
            Console.WriteLine($"Received message: SequenceNumber:{args.Message.SequenceNumber} Body:{Encoding.UTF8.GetString(args.Message.Body)}");

            // Complete the message so that it is not received again.
            // This can be done only if the queue Client is created in ReceiveMode.PeekLock mode (which is the default).
            await args.CompleteMessageAsync(args.Message);
        }

        // Use this handler to examine the exceptions received on the message pump.
        static Task ExceptionReceivedHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine($"Message handler encountered an exception {args.Exception.Message}");
            var context = args.ErrorSource;
            Console.WriteLine("Exception context for troubleshooting:");
            Console.WriteLine($"- Endpoint: {args.ErrorSource}");
            Console.WriteLine($"- Entity Path: {args.EntityPath}");
            Console.WriteLine($"- FQDN: {args.FullyQualifiedNamespace}");
            return Task.CompletedTask;
        }
    }
}
