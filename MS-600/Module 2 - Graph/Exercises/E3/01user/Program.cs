
namespace _01user
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

            // request 1 - all users
            var requestAllUsers = client.Users.Request();

            var results = requestAllUsers.GetAsync().Result;
            foreach (var user in results)
            {
                Console.WriteLine(user.Id + ": " + user.DisplayName + " <" + user.Mail + ">");
            }

            Console.WriteLine("\nGraph Request:");
            Console.WriteLine(requestAllUsers.GetHttpRequestMessage().RequestUri);

            // request 2 - current user
            var requestMeUser = client.Me.Request();

            var resultMe = requestMeUser.GetAsync().Result;
            Console.WriteLine(resultMe.Id + ": " + resultMe.DisplayName + " <" + resultMe.Mail + ">");

            Console.WriteLine("\nGraph Request:");
            Console.WriteLine(requestMeUser.GetHttpRequestMessage().RequestUri);

            // request 3 - specific user
            var requestSpecificUser = client.Users["AlexW@devtobi.onmicrosoft.com"].Request();
            var resultOtherUser = requestSpecificUser.GetAsync().Result;
            Console.WriteLine(resultOtherUser.Id + ": " + resultOtherUser.DisplayName + " <" + resultOtherUser.Mail + ">");

            Console.WriteLine("\nGraph Request:");
            Console.WriteLine(requestSpecificUser.GetHttpRequestMessage().RequestUri);
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


