
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

            var client = GetAuthenticatedHTTPClient(config, userName, userPassword);
            var totalRequests = 100;
            var successRequests = 0;
            var tasks = new List<Task>();
            var failResponseCode = HttpStatusCode.OK;
            HttpResponseHeaders failedHeaders = null!;

            for (int i = 0; i < totalRequests; i++)
            {
                tasks.Add(Task.Run(() =>
                {
                    var response = client.GetAsync("https://graph.microsoft.com/v1.0/me/messages").Result;
                    Console.Write(".");
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        successRequests++;
                    }
                    else
                    {
                        Console.Write('X');
                        failResponseCode = response.StatusCode;
                        failedHeaders = response.Headers;
                    }
                }));
            }

            var allWork = Task.WhenAll(tasks);
            try
            {
                allWork.Wait();
            }
            catch { }
            Console.WriteLine();
            Console.WriteLine("{0}/{1} requests succeeded.", successRequests, totalRequests);
            if (successRequests != totalRequests)
            {
                Console.WriteLine("Failed response code: {0}", failResponseCode.ToString());
                Console.WriteLine("Failed response headers: {0}", failedHeaders);
            }
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
    }
}


