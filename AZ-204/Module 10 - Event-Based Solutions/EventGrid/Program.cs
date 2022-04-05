using System;
using Microsoft.Azure.EventGrid;
using Microsoft.Azure.EventGrid.Models;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.IO;

namespace EventGrid
{
    class Program
    {
        static JObject azureConfig = JObject.Parse(File.ReadAllText("azureConfig.json"));

        // Batch Account credentials
        static String topicOrDomainEndpoint = azureConfig.GetValue("topicOrDomainEndpoint").ToString();
        static String topicOrDomainKey = azureConfig.GetValue("topicOrDomainKey").ToString();
        static String DomainTopic = "mysub";
        static void Main(string[] args)
        {
            
            string topicOrDomainHostname = new Uri(topicOrDomainEndpoint).Host;

            TopicCredentials topicCredentials = new TopicCredentials(topicOrDomainKey);
            EventGridClient client = new EventGridClient(topicCredentials);
            
            client.PublishEventsAsync(topicOrDomainHostname, GetEventsList()).GetAwaiter().GetResult();
            Console.Write("Published events to Event Grid.");

            static IList<EventGridEvent> GetEventsList()
            {
                List<EventGridEvent> eventsList = new List<EventGridEvent>();
                for (int i = 0; i < 10; i++)
                {
                    eventsList.Add(new EventGridEvent()
                    {
                        Id = Guid.NewGuid().ToString(),
                        Topic = DomainTopic,
                        EventType = "My.Custom.Event.Type.EventDomain",
                        Data = new Event() 
                        {
                            ItemInformation = "Some more information",
                            Schulung = "24.02.2022 11:17 Uhr"
                        },

                        EventTime = DateTime.Now,
                        Subject = "My custom subject 24.02.2022",
                        DataVersion = "2.011256"
                    });
                }
                return eventsList;
            }
        }
    }
}
