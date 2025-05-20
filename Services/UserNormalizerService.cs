using AnalyzerApp.Models;

namespace AnalyzerApp.Services;

public static class UserNormalizerService
{
    public static List<NormalizedUser> NormalizeUsers(
        List<B2CUser> b2cUsers,
        List<CosmosUser> cosmosUsers,
        List<SqlUser> sqlUsers,
        out List<B2CUser> b2cOrphans,
        out List<SqlUser> sqlOrphans)
    {
        var normalized = new List<NormalizedUser>();
        b2cOrphans = new();
        sqlOrphans = new();

        var matchedB2CIds = new HashSet<Guid>();
        var matchedSqlIds = new HashSet<Guid>();

        // Create lookups
        var b2cById = b2cUsers
            .Where(b => b.Id != null)
            .ToDictionary(b => b.Id!.ToString());
        var sqlByUsername = sqlUsers
            .Where(s => !string.IsNullOrWhiteSpace(s.Username))
            .ToDictionary(s => s.Username!.Trim().ToLower());

        // Step 1 & 2: Cosmos → B2C and IDS3
        foreach (var cosmos in cosmosUsers)
        {
            var user = new NormalizedUser
            {
                CosmosId = cosmos.id?.ToString() ?? Guid.Empty.ToString(),
                Email = cosmos.Email,
                Username = cosmos.Username,
                DisplayName = cosmos.DisplayName,
                ExistsInCosmos = true
            };

            // Match to B2C by B2CId
            if (!string.IsNullOrEmpty(cosmos.B2CId) && b2cById.TryGetValue(cosmos.B2CId, out var b2c))
            {
                if (Guid.TryParse(b2c.Id, out var parsedB2CId))
                {
                    user.B2CId = parsedB2CId.ToString();
                    matchedB2CIds.Add(parsedB2CId);
                }
                user.ExistsInB2C = true;
            }

            // Match to IDS3 by Email or Username (case-insensitive)
            string?[] lookupKeys = new[]
            {
                cosmos.Email?.Trim().ToLower(),
                cosmos.Username?.Trim().ToLower()
            };

            SqlUser? sql = null;
            foreach (var key in lookupKeys.Where(k => !string.IsNullOrWhiteSpace(k)))
            {
                if (sqlByUsername.TryGetValue(key!, out sql))
                    break;
            }

            if (sql != null)
            {
                if (Guid.TryParse(sql.PublicKey, out var parsedSqlId))
                {
                    user.IDS3Id = parsedSqlId.ToString();
                    matchedSqlIds.Add(parsedSqlId);
                }
                user.ExistsInSql = true;
            }

            normalized.Add(user);
        }

        // Step 3: Match unmatched B2C → IDS3
        foreach (var b2c in b2cUsers.Where(b => 
            Guid.TryParse(b.Id, out var guid) && !matchedB2CIds.Contains(guid)))
        {
            var user = new NormalizedUser
            {
                CosmosId = b2c.Id != null ? b2c.Id.ToString() : Guid.Empty.ToString(),
                Email = b2c.UserPrincipalName,
                Username = b2c.UserPrincipalName,
                DisplayName = b2c.DisplayName,
                B2CId = Guid.TryParse(b2c.Id, out var parsedB2CId) ? parsedB2CId.ToString() : null,
                ExistsInB2C = true
            };

            var emailKey = b2c.UserPrincipalName?.Trim().ToLower();
            if (emailKey != null && sqlByUsername.TryGetValue(emailKey, out var sql))
            {
                if (Guid.TryParse(sql.PublicKey, out var parsedSqlIdStep3))
                {
                    user.IDS3Id = parsedSqlIdStep3.ToString();
                    matchedSqlIds.Add(parsedSqlIdStep3);
                }
                user.ExistsInSql = true;
            }

            normalized.Add(user);
        }

        // Step 4: Orphans
        b2cOrphans = b2cUsers
            .Where(b => Guid.TryParse(b.Id, out var guid) && !matchedB2CIds.Contains(guid))
            .ToList();

        sqlOrphans = sqlUsers
            .Where(s => {
                if (Guid.TryParse(s.PublicKey, out var guid))
                    return !matchedSqlIds.Contains(guid);
                return true;
            })
            .ToList();

        return normalized;
    }
}
