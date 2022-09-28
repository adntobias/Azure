using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Logging;

namespace managed_identity_demo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SecretController : ControllerBase
    {
        private readonly ILogger<SecretController> _logger;

        public SecretController(ILogger<SecretController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public async Task<string> Get()
        {
            //SDK für Managed Identitites
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            //Key Vault Client mit Authentication
            var kv = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
            //KeyVault Client um Secrets abzurufen

            string secret = "";
            
            try
            {
                secret = (await kv.GetSecretAsync("https://managedidentkvtp2909.vault.azure.net/", "supergeheim")).Value;
            }
            catch
            {
                secret = "Not Authorized";
            }

            return secret;
        }
    }
}