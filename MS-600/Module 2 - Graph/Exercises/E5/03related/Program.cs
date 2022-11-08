
namespace _01onedrive
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
            var client = GetAuthenticatedGraphClient(config, userName, userPassword);

            // request 1 - get trending files around a specific user (me)
            var requestt = client.Me.Insights.Trending.Request();

            var resultst = requestt.GetAsync().Result;
            foreach (var resource in resultst)
            {
                Console.WriteLine("(" + resource.ResourceVisualization.Type + ") - " + resource.ResourceVisualization.Title);
                Console.WriteLine("  Weight: " + resource.Weight);
                Console.WriteLine("  Id: " + resource.Id);
                Console.WriteLine("  ResourceId: " + resource.ResourceReference.Id);
            }

            // request 2 - used files
            var requestu = client.Me.Insights.Used.Request();

            var resultsu = requestu.GetAsync().Result;
            foreach (var resource in resultsu)
            {
                Console.WriteLine("(" + resource.ResourceVisualization.Type + ") - " + resource.ResourceVisualization.Title);
                Console.WriteLine("  Last Accessed: " + resource.LastUsed.LastAccessedDateTime.ToString());
                Console.WriteLine("  Last Modified: " + resource.LastUsed.LastModifiedDateTime.ToString());
                Console.WriteLine("  Id: " + resource.Id);
                Console.WriteLine("  ResourceId: " + resource.ResourceReference.Id);
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
            scopes.Add("User.Read.All");
            scopes.Add("User.ReadBasic.All");
            scopes.Add("Group.Read.All");
            scopes.Add("Directory.Read.All");
            scopes.Add("Files.Read");
            scopes.Add("Sites.Read.All");

            var cca = PublicClientApplicationBuilder.Create(clientId)
                                                    .WithAuthority(authority)
                                                    .Build();
            return MsalAuthenticationProvider.GetInstance(cca, scopes.ToArray(), userName, userPassword);
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
    }
}


