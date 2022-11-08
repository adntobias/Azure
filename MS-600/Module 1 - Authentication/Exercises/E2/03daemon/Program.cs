using Helpers;

var graphHandler = new GraphHandler();
var config = graphHandler.LoadAppSettings();
if (config == null)
{
  Console.WriteLine("Invalid appsettings.json file.");
  return;
}

var client = graphHandler.GetAuthenticatedGraphClient(config);

var requestUserEmail = client.Users[config["targetUserId"]].Messages.Request();
var results = requestUserEmail.GetAsync().Result;
foreach (var message in results)
{
  Console.WriteLine("");
  Console.WriteLine("Subject : " + message.Subject);
  Console.WriteLine("Received: " + message.ReceivedDateTime.ToString());
  Console.WriteLine("ID      : " + message.Id);
}

Console.WriteLine("\nGraph Request:");
Console.WriteLine(requestUserEmail.GetHttpRequestMessage().RequestUri);