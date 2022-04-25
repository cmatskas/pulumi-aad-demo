using System.Threading.Tasks;
using Pulumi;
using Pulumi.AzureNative.Resources;
using AzureNative = Pulumi.AzureNative;
using Azure.Security.KeyVault.Secrets;
using Azure.Identity;
using Azure.Core;
using System;

class MyStack : Stack
{
    public MyStack()
    {
        // Create an Azure Resource Group
        var resourceGroup = new ResourceGroup("pulumi-cs-demo");

        // Create an Azure resource (Storage Account)
        var server = new AzureNative.Sql.Server("server", new AzureNative.Sql.ServerArgs
        {
            AdministratorLogin = GetkeyVaultSecret("sqlAdministratorLogin").GetAwaiter().GetResult(),
            AdministratorLoginPassword = GetkeyVaultSecret("sqlAdministratorLoginPassword").GetAwaiter().GetResult(),
            Location = resourceGroup.Location,
            ResourceGroupName = resourceGroup.Name,
            ServerName = $"sqlpulumidemo{Guid.NewGuid().ToString().Substring(0,4)}"
        }); 
    }

    private static async Task<string> GetkeyVaultSecret(string secretName)
    {
        var credential = new DefaultAzureCredential();
        var token = credential.GetToken(new Azure.Core.TokenRequestContext(new [] {"https://vault.azure.net/.default"}));
        var tokenCredential = DelegatedTokenCredential.Create((_,_) => token);
        var client = new SecretClient(new Uri("https://cm-identity-kv.vault.azure.net"), tokenCredential);

        var secret = (await client.GetSecretAsync(secretName)).Value;
        return secret.Value;
    }
}
