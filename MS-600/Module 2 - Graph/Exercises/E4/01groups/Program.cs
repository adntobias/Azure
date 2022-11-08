
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

            // request 1 - all groups
            Console.WriteLine("\n\nREQUEST 1 - ALL GROUPS:");
            var requestAllGroups = client.Groups.Request();
            var resultsAllGroups = requestAllGroups.GetAsync().Result;
            foreach (var group in resultsAllGroups)
            {
                Console.WriteLine(group.Id + ": " + group.DisplayName + " <" + group.Mail + ">");
            }

            Console.WriteLine("\nGraph Request:");
            Console.WriteLine(requestAllGroups.GetHttpRequestMessage().RequestUri);

            var groupId = "a1531e24-0da8-400b-8ba1-c9f6b2f7a9aa";
            // request 2 - one group
            Console.WriteLine("\n\nREQUEST 2 - ONE GROUP:");
            var requestGroup = client.Groups[groupId].Request();
            var resultsGroup = requestGroup.GetAsync().Result;
            Console.WriteLine(resultsGroup.Id + ": " + resultsGroup.DisplayName + " <" + resultsGroup.Mail + ">");

            Console.WriteLine("\nGraph Request:");
            Console.WriteLine(requestGroup.GetHttpRequestMessage().RequestUri);

            // request 3 - group owners
            Console.WriteLine("\n\nREQUEST 3 - GROUP OWNERS:");
            var requestGroupOwners = client.Groups[groupId].Owners.Request();
            var resultsGroupOwners = requestGroupOwners.GetAsync().Result;
            foreach (var owner in resultsGroupOwners)
            {
                var ownerUser = owner as Microsoft.Graph.User;
                if (ownerUser != null)
                {
                    Console.WriteLine(ownerUser.Id + ": " + ownerUser.DisplayName + " <" + ownerUser.Mail + ">");
                }
            }

            Console.WriteLine("\nGraph Request:");
            Console.WriteLine(requestGroupOwners.GetHttpRequestMessage().RequestUri);

            // request 4 - group members
            Console.WriteLine("\n\nREQUEST 4 - GROUP MEMBERS:");
            var requestGroupMembers = client.Groups[groupId].Members.Request();
            var resultsGroupMembers = requestGroupMembers.GetAsync().Result;
            foreach (var member in resultsGroupMembers)
            {
                var memberUser = member as Microsoft.Graph.User;
                if (memberUser != null)
                {
                    Console.WriteLine(memberUser.Id + ": " + memberUser.DisplayName + " <" + memberUser.Mail + ">");
                }
            }

            Console.WriteLine("\nGraph Request:");
            Console.WriteLine(requestGroupMembers.GetHttpRequestMessage().RequestUri);
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


