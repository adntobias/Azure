﻿
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

            // request 1 - upload small file to user's onedrive
            var fileNamexs = "smallfile.txt";
            var filePathxs = Path.Combine(System.IO.Directory.GetCurrentDirectory(), fileNamexs);
            Console.WriteLine("Uploading file: " + fileNamexs);

            FileStream fileStream = new FileStream(filePathxs, FileMode.Open);
            var uploadedFile = client.Me.Drive.Root
                                          .ItemWithPath("smallfile.txt")
                                          .Content
                                          .Request()
                                          .PutAsync<DriveItem>(fileStream)
                                          .Result;
            Console.WriteLine("File uploaded to: " + uploadedFile.WebUrl);

            // request 2 - upload large file to user's onedrive
            var fileNamexl = "largefile.zip";
            var filePathxl = Path.Combine(System.IO.Directory.GetCurrentDirectory(), fileNamexl);
            Console.WriteLine("Uploading file: " + fileNamexl);

            // load resource as a stream
            using (Stream stream = new FileStream(filePathxl, FileMode.Open))
            {
                var uploadSession = client.Me.Drive.Root
                                                .ItemWithPath(fileNamexl)
                                                .CreateUploadSession()
                                                .Request()
                                                .PostAsync()
                                                .Result;

                // create upload task
                var maxChunkSize = 320 * 1024;
                var largeUploadTask = new LargeFileUploadTask<DriveItem>(uploadSession, stream, maxChunkSize);

                // create progress implementation
                IProgress<long> uploadProgress = new Progress<long>(uploadBytes =>
                {
                    Console.WriteLine($"Uploaded {uploadBytes} bytes of {stream.Length} bytes");
                });

                // upload file
                UploadResult<DriveItem> uploadResult = largeUploadTask.UploadAsync(uploadProgress).Result;
                if (uploadResult.UploadSucceeded)
                {
                    Console.WriteLine("File uploaded to user's OneDrive root folder.");
                }
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
            scopes.Add("Files.ReadWrite");

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


