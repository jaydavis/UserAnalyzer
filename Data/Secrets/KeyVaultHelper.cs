using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using System.Threading.Tasks;

namespace AnalyzerApp.Data.Secrets
{
    public static class KeyVaultHelper
    {
        public static async Task<string> GetSecretAsync(string vaultName, string secretName)
        {
            var client = new SecretClient(
                new Uri($"https://{vaultName}.vault.azure.net/"),
                new DefaultAzureCredential()
            );

            var response = await client.GetSecretAsync(secretName);
            return response.Value.Value;
        }
    }
}
