using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.IO;

namespace QueueStorage
{
    class Program
    {

        static JObject azureConfig = JObject.Parse(File.ReadAllText("azureConfig.json"));

        // Batch Account credentials
        static String connectionString = azureConfig.GetValue("connectionString").ToString();
        
        static void Main(string[] args)
        {
            AsyncMain(args).GetAwaiter().GetResult();
        }

        static async Task AsyncMain(string[] args) {
            string queueName = "quickstartqueues-f110e125-aa27-4cac-98d5-7f62ff2836db" ;//+ Guid.NewGuid().ToString();

            Console.WriteLine($"Creating queue: {queueName}");

            // Instantiate a QueueClient which will be
            // used to create and manipulate the queue
            QueueClient queueClient = new QueueClient(connectionString, queueName);

            // Create the queue
            await queueClient.CreateAsync();

            Console.WriteLine("\nAdding messages to the queue...");

            // Send several messages to the queue
            SendReceipt receipt = await queueClient.SendMessageAsync("First message");
            await queueClient.SendMessageAsync("Second message");

            // Save the receipt so we can update this message later
            await queueClient.SendMessageAsync("Third message");

            Console.WriteLine("\nPeek at the messages in the queue...");

            // Peek at messages in the queue
            PeekedMessage[] peekedMessages = await queueClient.PeekMessagesAsync(maxMessages: 10);

            foreach (PeekedMessage peekedMessage in peekedMessages)
            {
                // Display the message
                Console.WriteLine($"Message: {peekedMessage.MessageText}");
            }

            Console.WriteLine("Press enter to update messages in the queue");
            Console.ReadLine();
            Console.WriteLine("\nUpdating the third message in the queue...");

            // Update a message using the saved receipt from sending the message
            await queueClient.UpdateMessageAsync(receipt.MessageId, receipt.PopReceipt, "First message has been updated");

            Console.WriteLine("\nReceiving messages from the queue...");

            // Get messages from the queue
            QueueMessage[] messages = await queueClient.ReceiveMessagesAsync(maxMessages: 10);

            Console.WriteLine("\nPress Enter key to 'process' messages and delete them from the queue...");
            Console.ReadLine();

            // Process and delete messages from the queue
            foreach (QueueMessage message in messages)
            {
                // "Process" the message
                Console.WriteLine($"Message: {message.MessageText}");

                // Let the service know we're finished with
                // the message and it can be safely deleted.
                await queueClient.DeleteMessageAsync(message.MessageId, message.PopReceipt);
            }

            Console.WriteLine("\nPress Enter key to delete the queue...");
            Console.ReadLine();

            // Clean up
            Console.WriteLine($"Deleting queue: {queueClient.Name}");
            
            await queueClient.DeleteAsync();

            Console.WriteLine("Done");
        }
    }
}
