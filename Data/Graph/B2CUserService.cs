using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Users;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AnalyzerApp.Data.Graph
{
    public class B2CUserService
    {
        private readonly GraphServiceClient _graphClient;

        public B2CUserService(GraphServiceClient graphClient)
        {
            _graphClient = graphClient;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            var users = new List<User>();
            var page = await _graphClient.Users.GetAsync(config =>
            {
                config.QueryParameters.Select = new[] {
                    "id", "objectId", "displayName", "userPrincipalName", "givenName", "surname", "identities"
                };
                config.QueryParameters.Top = 999; // Max allowed per page
            });

            if (page?.Value != null)
                users.AddRange(page.Value);

            var nextLink = page?.OdataNextLink;

            while (!string.IsNullOrEmpty(nextLink))
            {
                var nextPage = await _graphClient.Users.WithUrl(nextLink).GetAsync();
                if (nextPage?.Value != null)
                    users.AddRange(nextPage.Value);

                nextLink = nextPage?.OdataNextLink;
            }

            return users;
        }

    }
}
