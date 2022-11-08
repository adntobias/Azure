
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

            // request 1 - get user's files
            var request = client.Me.Drive.Root.Children.Request();

            var results = request.GetAsync().Result;
            foreach (var file in results)
            {
                Console.WriteLine(file.Id + ": " + file.Name);
            }

            // request 2 - get specific file
            var fileId = "01AXAOJZHJ4AGWT7CJMJEYOTG5CQH5SFUU";
            var requestread = client.Me.Drive.Items[fileId].Request();

            var resultread = requestread.GetAsync().Result;
            Console.WriteLine(resultread.Id + ": " + resultread.Name);

            // request 3 - download specific file
            fileId = "01AXAOJZEOYKBMCGXPMBEIAPJMN4ETM3FM";
            var requestload = client.Me.Drive.Items[fileId].Content.Request();

            var stream = requestload.GetAsync().Result;
            var driveItemPath = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "driveItem_" + fileId + ".file");
            var driveItemFile = System.IO.File.Create(driveItemPath);
            stream.Seek(0, SeekOrigin.Begin);
            stream.CopyTo(driveItemFile);
            Console.WriteLine("Saved file to: " + driveItemPath);

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


