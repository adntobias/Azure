
namespace _01groups
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

            // request 1 - all groups member of
            Console.WriteLine("\n\nREQUEST 1 - ALL GROUPS MEMBER OF:");
            var requestGroupsMemberOf = client.Me.MemberOf.Request();
            var resultsGroupsMemberOf = requestGroupsMemberOf.GetAsync().Result;

            foreach (var groupDirectoryObject in resultsGroupsMemberOf)
            {
                var group = groupDirectoryObject as Microsoft.Graph.Group;
                var role = groupDirectoryObject as Microsoft.Graph.DirectoryRole;
                if (group != null)
                {
                    Console.WriteLine("Group: " + group.Id + ": " + group.DisplayName);
                }
                else if (role != null)
                {
                    Console.WriteLine("Role: " + role.Id + ": " + role.DisplayName);
                }
                else
                {
                    Console.WriteLine(groupDirectoryObject.ODataType + ": " + groupDirectoryObject.Id);
                }
            }

            // request 2 - all groups owner of
            Console.WriteLine("\n\nREQUEST 2 - ALL GROUPS OWNER OF:");
            var requestOwnerOf = client.Me.OwnedObjects.Request();
            var resultsOwnerOf = requestOwnerOf.GetAsync().Result;
            foreach (var ownedObject in resultsOwnerOf)
            {
                var group = ownedObject as Microsoft.Graph.Group;
                var role = ownedObject as Microsoft.Graph.DirectoryRole;
                if (group != null)
                {
                    Console.WriteLine("Office 365 Group: " + group.Id + ": " + group.DisplayName);
                }
                else if (role != null)
                {
                    Console.WriteLine("  Security Group: " + role.Id + ": " + role.DisplayName);
                }
                else
                {
                    Console.WriteLine(ownedObject.ODataType + ": " + ownedObject.Id);
                }
            }

            Console.WriteLine("\nGraph Request:");
            Console.WriteLine(requestOwnerOf.GetHttpRequestMessage().RequestUri);


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


