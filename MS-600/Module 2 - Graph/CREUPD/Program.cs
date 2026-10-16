using System;
using System.Threading;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Newtonsoft.Json.Linq;
using Azure.Identity;

namespace CREUPD
{
    class Program
    {
        static JObject azureConfig = JObject.Parse(System.IO.File.ReadAllText("appsettings.json"));
        static String clientID = azureConfig.GetValue("CLIENT_ID").ToString();
        static String tenantID = azureConfig.GetValue("TENANT_ID").ToString();
        static String secret = azureConfig.GetValue("SECRET").ToString();

        //Permissions: User.Read, User.ReadWrite.All
        //private static List<string> scopes = new List<string>();

        static void Main(string[] args)
        {
            var scopes = new[] { "https://graph.microsoft.com/.default" };
            //scopes.Add("https://graph.microsoft.com/.default");

            var options = new ClientSecretCredentialOptions
            {
                AuthorityHost = AzureAuthorityHosts.AzurePublicCloud,
            };

            var clientSecretCredential = new ClientSecretCredential(tenantID, clientID, secret, options);

            var graphServiceClient = new GraphServiceClient(clientSecretCredential, scopes);

            //Wird später zum neuen Manager
            var manager = graphServiceClient.Users["mctest@devtobi.onmicrosoft.com"].GetAsync().GetAwaiter().GetResult();

            var newUser = new User
            {
                DisplayName = "Test User",
                UserPrincipalName = "testu@devtobi.onmicrosoft.com",
                MailNickname = "testu",
                AccountEnabled = true,
                PasswordProfile = new PasswordProfile
                {
                    Password = "ChangeMe1!",
                    ForceChangePasswordNextSignIn = false
                },
                GivenName = "Test",
                Department = "TestGuys",
                Surname = "User"
            };

            var timedKillCommand = new Timer((e) =>
            {
                Console.WriteLine("Killing The User");
                try
                {
                    graphServiceClient.Users[newUser.UserPrincipalName].DeleteAsync().GetAwaiter();

                    Console.WriteLine("Assimilated");
                }
                catch (Exception e1)
                {
                    Console.WriteLine($"ERROR: {e1.Message}");
                }
                finally
                {
                    Environment.Exit(0);
                }
            }, null, ((1000 * 60) * 1), Timeout.Infinite);

            //User anlegen
            try
            {
                var userCall = graphServiceClient.Users.PostAsync(newUser).GetAwaiter().GetResult();

                Console.WriteLine("User created? I\'m checking");
                
                var checkUser = graphServiceClient.Users.GetAsync(config => {
                    config.QueryParameters.Select = ["DisplayName", "Department", "Id", "Manager"];
                    config.QueryParameters.Filter = $"displayName eq '{newUser.DisplayName}'";
                }).GetAwaiter().GetResult();

                if (checkUser == null)
                {
                    Console.WriteLine("User NOT created!");
                    return;
                }

                while(checkUser.Value != null)
                {
                    var user = checkUser.Value[0];

                    Console.WriteLine($"User: {user.DisplayName}");

                    Console.WriteLine("User created! Let\'s assign a manager");
                    graphServiceClient.Users[user.Id].Manager.Ref.PutAsync(new ReferenceUpdate
                    {
                        OdataId = $"https://graph.microsoft.com/v1.0/users/{manager.Id}"
                    }).GetAwaiter();

                    Console.WriteLine("Manager Assigned! Let\'s rename him ....");
                    var userUpdate = new User
                    {
                        DisplayName = "Updated thisGuy",
                        OfficeLocation = "Knowhere"
                    };

                    var updatedUser = graphServiceClient.Users[user.Id].PatchAsync(userUpdate).GetAwaiter().GetResult();
                    if (updatedUser == null)
                    {
                        Console.WriteLine($"{userUpdate.DisplayName} working at {userUpdate.OfficeLocation}");
                    }
                    else
                    {
                        Console.WriteLine("User not updated!");
                    }

                    //checkUserContent = graphServiceClient.Users[user.Id].GetAsync(config =>
                    //{
                    //    config.QueryParameters.Select = ["DisplayName", "Department", "Id"];
                    //}).GetAwaiter().GetResult();
                    break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"ERROR: {e.Message}");
            }

            Console.WriteLine("HIT ME");
            Console.ReadLine();
        }
    }
}

