using AnalyzerApp.Models;
using System.Collections.Generic;
using System.Linq;

namespace AnalyzerApp.Services
{
    public static class UserNormalizerService
    {
        public static List<NormalizedUser> NormalizeUsers(
            List<B2CUser> b2cUsers,
            List<CosmosUser> cosmosUsers,
            List<SqlUser> sqlUsers)
        {
            var normalized = new Dictionary<string, NormalizedUser>();

            // 1. Add from B2C
            foreach (var b2c in b2cUsers)
            {
                var key = b2c.Id ?? b2c.UserPrincipalName ?? $"b2c:{Guid.NewGuid()}";

                normalized[key] = new NormalizedUser
                {
                    CommonId = key,
                    DisplayName = b2c.DisplayName,
                    Username = b2c.UserPrincipalName,
                    Email = b2c.UserPrincipalName,
                    ExistsInB2C = true
                };
            }

            // 2. Merge Cosmos Users by Email
            foreach (var cosmos in cosmosUsers)
            {
                var matchKey = normalized.Values.FirstOrDefault(u =>
                    u.Email?.ToLower() == cosmos.Email?.ToLower() ||
                    u.Username?.ToLower() == cosmos.Email?.ToLower());

                if (matchKey != null)
                {
                    matchKey.ExistsInCosmos = true;
                    if (string.IsNullOrWhiteSpace(matchKey.DisplayName))
                        matchKey.DisplayName = $"{cosmos.FirstName} {cosmos.LastName}".Trim();
                }
                else
                {
                    var key = cosmos.Email ?? $"cosmos:{Guid.NewGuid()}";
                    normalized[key] = new NormalizedUser
                    {
                        CommonId = key,
                        DisplayName = $"{cosmos.FirstName} {cosmos.LastName}".Trim(),
                        Email = cosmos.Email,
                        Username = cosmos.Email,
                        ExistsInCosmos = true
                    };
                }
            }

            // 3. Merge SQL Users by ID or Username
            foreach (var sql in sqlUsers)
            {
                // Match by PublicKey OR Username if email matches
                var matchKey = normalized.Values.FirstOrDefault(u =>
                    (!string.IsNullOrWhiteSpace(sql.PublicKey) && u.CommonId == sql.PublicKey) ||
                    (u.Username?.ToLower() == sql.Username?.ToLower()));

                if (matchKey != null)
                {
                    matchKey.ExistsInSql = true;
                    if (string.IsNullOrWhiteSpace(matchKey.Username))
                        matchKey.Username = sql.Username;
                    if (string.IsNullOrWhiteSpace(matchKey.CommonId))
                        matchKey.CommonId = sql.PublicKey ?? matchKey.CommonId;
                }
                else
                {
                    var key = sql.PublicKey ?? $"sql:{Guid.NewGuid()}";
                    normalized[key] = new NormalizedUser
                    {
                        CommonId = key,
                        Username = sql.Username,
                        ExistsInSql = true
                    };
                }
            }

            return normalized.Values.ToList();
        }
    }
}
