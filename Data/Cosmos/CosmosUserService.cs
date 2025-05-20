using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;
using AnalyzerApp.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnalyzerApp.Data.Cosmos
{
    public class CosmosUserService
    {
        private readonly CosmosClient _client;
        private readonly Container _container;

        public CosmosUserService(string connectionString, string databaseName, string containerName)
        {
            _client = new CosmosClient(connectionString);
            _container = _client.GetContainer(databaseName, containerName);
        }

        public async Task<List<CosmosUser>> GetUsersAsync()
        {
            var query = new QueryDefinition("SELECT * FROM c");
            var results = new List<CosmosUser>();

            var iterator = _container.GetItemQueryIterator<JObject>(query);

            while (iterator.HasMoreResults)
            {
                foreach (var item in await iterator.ReadNextAsync())
                {
                    var groups = item["Groups"]?.ToObject<List<UserGroup>>() ?? new List<UserGroup>();
                    var claims = item["UserClaims"]?.ToObject<List<UserClaim>>() ?? new List<UserClaim>();

                    results.Add(new CosmosUser
                    {
                        FirstName = item["FirstName"]?.ToString(),
                        LastName = item["LastName"]?.ToString(),
                        Email = item["Email"]?.ToString(),
                        id = item["id"]?.ToString(),
                        B2CId = item["B2CId"]?.ToString(),
                        ClientId = item["ClientGuid"]?.ToString(),
                        IdpId = item["IdentityProviderGuid"]?.ToString(),
                        IDS3Enabled = item["IsIDS3EnabledFlag"]?.ToObject<bool>() ?? false,
                        B2CEnabled = item["IsB2CEnabledFlag"]?.ToObject<bool>() ?? false,
                        B2CIssuer = item["B2CIssuer"]?.ToString(),
                        Groups = groups,
                        UserClaims = claims
                    });
                }
            }

            return results;    
        }
    }
}
