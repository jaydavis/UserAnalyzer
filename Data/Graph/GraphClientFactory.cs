using Azure.Identity;
using Microsoft.Graph;

namespace AnalyzerApp.Data.Graph
{
    public static class GraphClientFactory
    {
        public static GraphServiceClient Create(string tenantId, string clientId, string clientSecret)
        {
            var credential = new ClientSecretCredential(
                tenantId,
                clientId,
                clientSecret
            );

            return new GraphServiceClient(credential, new[] { "https://graph.microsoft.com/.default" });
        }
    }
}
