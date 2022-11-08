
namespace _01throttling
{
    class Program
    {
        private static GraphServiceClient? _graphClient;

        static void Main(string[] args)
        {
            var config = LoadAppSettings();
            if (config == null)
            {
                Console.WriteLine("Invalid appsettings.json file.");
                return;
            }

            var userName = ReadUsername();
            var userPassword = ReadPassword();

            //var client = GetAuthenticatedHTTPClient(config, userName, userPassword); //01
            var client = GetAuthenticatedGraphClient(config, userName, userPassword);
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            /* 01
            var clientResponse = client.GetAsync("https://graph.microsoft.com/v1.0/me/messages?$select=id&$top=100").Result;
            // enumerate through the list of messages
            var httpResponseTask = clientResponse.Content.ReadAsStringAsync();
            httpResponseTask.Wait();
            var graphMessages = JsonSerializer.Deserialize<Messages>(httpResponseTask.Result);
            var items = graphMessages == null ? Array.Empty<Message>() : graphMessages.Items;
            */
            var clientResponse = client.Me.Messages
                              .Request()
                              .Select(m => new { m.Id })
                              .Top(100)
                              .GetAsync()
                              .Result;
            var items = clientResponse.CurrentPage;

            var tasks = new List<Task>();
            foreach (var graphMessage in items)
            {
                tasks.Add(Task.Run(() =>
                {
                    Console.WriteLine("...retrieving message: {0}", graphMessage.Id);

                    var messageDetail = GetMessageDetailV2(client, graphMessage.Id);

                    if (messageDetail != null)
                    {
                        Console.WriteLine("SUBJECT: {0}", messageDetail.Subject);
                    }
                }));
            }

            // do all work in parallel & wait for it to complete
            var allWork = Task.WhenAll(tasks);
            try
            {
                allWork.Wait();
            }
            catch { }

            stopwatch.Stop();
            Console.WriteLine();
            Console.WriteLine("Elapsed time: {0} seconds", stopwatch.Elapsed.Seconds);
        }

        private static IConfigurationRoot? LoadAppSettings()
        {
            try
            {
                var config = new ConfigurationBuilder()
                                  .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                                  .AddJsonFile("appsettings.json", false, true)
                                  .Build();

                if (string.IsNullOrEmpty(config["applicationId"]) ||
                    string.IsNullOrEmpty(config["applicationSecret"]) ||
                    string.IsNullOrEmpty(config["redirectUri"]) ||
                    string.IsNullOrEmpty(config["tenantId"]))
                {
                    return null;
                }

                return config;
            }
            catch (System.IO.FileNotFoundException)
            {
                return null;
            }
        }

        private static IAuthenticationProvider CreateAuthorizationProvider(IConfigurationRoot config, string userName, SecureString userPassword)
        {
            var clientId = config["applicationId"];
            var authority = $"https://login.microsoftonline.com/{config["tenantId"]}/v2.0";

            List<string> scopes = new List<string>();
            scopes.Add("User.Read");
            scopes.Add("Mail.Read");

            var cca = PublicClientApplicationBuilder.Create(clientId)
                                                    .WithAuthority(authority)
                                                    .Build();
            return MsalAuthenticationProvider.GetInstance(cca, scopes.ToArray(), userName, userPassword);
        }

        private static HttpClient GetAuthenticatedHTTPClient(IConfigurationRoot config, string userName, SecureString userPassword)
        {
            var authenticationProvider = CreateAuthorizationProvider(config, userName, userPassword);
            var httpClient = new HttpClient(new AuthHandler(authenticationProvider, new HttpClientHandler()));
            return httpClient;
        }

        private static GraphServiceClient GetAuthenticatedGraphClient(IConfigurationRoot config, string userName, SecureString userPassword)
        {
            var authenticationProvider = CreateAuthorizationProvider(config, userName, userPassword);
            var graphClient = new GraphServiceClient(authenticationProvider);
            return graphClient;
        }

        private static SecureString ReadPassword()
        {
            Console.WriteLine("Enter your password");
            SecureString password = new SecureString();
            while (true)
            {
                ConsoleKeyInfo c = Console.ReadKey(true);
                if (c.Key == ConsoleKey.Enter)
                {
                    break;
                }
                password.AppendChar(c.KeyChar);
                Console.Write("*");
            }
            Console.WriteLine();
            return password;
        }

        private static string ReadUsername()
        {
            string? username;
            Console.WriteLine("Enter your username");
            username = Console.ReadLine();
            return username ?? "";
        }

        private static Message? GetMessageDetail(HttpClient client, string messageId, int defaultDelay = 2)
        {
            Message? messageDetail = null;

            string endpoint = "https://graph.microsoft.com/v1.0/me/messages/" + messageId;

            // submit request to Microsoft Graph & wait to process response
            var clientResponse = client.GetAsync(endpoint).Result;
            var httpResponseTask = clientResponse.Content.ReadAsStringAsync();
            httpResponseTask.Wait();

            Console.WriteLine("...Response status code: {0}  ", clientResponse.StatusCode);

            // IF request successful (not throttled), set message to retrieved message
            if (clientResponse.StatusCode == HttpStatusCode.OK)
            {
                messageDetail = JsonSerializer.Deserialize<Message>(httpResponseTask.Result);
            }
            // ELSE IF request was throttled (429, aka: TooManyRequests)...
            else if (clientResponse.StatusCode == HttpStatusCode.TooManyRequests)
            {
                // get retry-after if provided; if not provided default to 2s
                var retryAfterDelay = defaultDelay;
                var retryAfter = clientResponse.Headers.RetryAfter;
                if (retryAfter != null && retryAfter.Delta.HasValue && (retryAfter.Delta.Value.Seconds > 0))
                {
                    retryAfterDelay = retryAfter.Delta.Value.Seconds;
                }

                // wait for specified time as instructed by Microsoft Graph's Retry-After header,
                //    or fall back to default
                Console.WriteLine(">>>>>>>>>>>>> sleeping for {0} seconds...", retryAfterDelay);
                System.Threading.Thread.Sleep(retryAfterDelay * 1000);

                // call method again after waiting
                messageDetail = GetMessageDetail(client, messageId);
            }

            return messageDetail;
        }

        private static Microsoft.Graph.Message GetMessageDetailV2(GraphServiceClient client, string messageId)
        {
            // submit request to Microsoft Graph & wait to process response
            return client.Me.Messages[messageId].Request().GetAsync().Result;
        }
    }
}


