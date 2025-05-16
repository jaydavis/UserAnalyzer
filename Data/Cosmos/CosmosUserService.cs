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
            var query = new QueryDefinition("SELECT c.FirstName, c.LastName, c.Email FROM c WHERE c.IsB2CEnabledFlag = true");
            var results = new List<CosmosUser>();

            var iterator = _container.GetItemQueryIterator<JObject>(query);

            while (iterator.HasMoreResults)
            {
                foreach (var item in await iterator.ReadNextAsync())
                {
                    results.Add(new CosmosUser
                    {
                        FirstName = item["FirstName"]?.ToString(),
                        LastName = item["LastName"]?.ToString(),
                        Email = item["Email"]?.ToString()
                    });
                }
            }

            return results;    
        }
    }
}
