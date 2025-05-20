using AnalyzerApp.Models;
using System.Collections.Generic;
using System.Linq;

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

        var matchedB2CIds = new HashSet<string>();
        var matchedSqlIds = new HashSet<string>();

        foreach (var cosmosUser in cosmosUsers)
        {
            var cosmosId = cosmosUser?.id ?? string.Empty;
            var b2cId = cosmosUser?.B2CId ?? string.Empty;

            var b2cUser = b2cUsers.FirstOrDefault(u => u?.Id == b2cId);
            if (!string.IsNullOrWhiteSpace(b2cUser?.Id))
                matchedB2CIds.Add(b2cUser.Id);

            var sqlUser = sqlUsers.FirstOrDefault(u => u?.B2CId == b2cId);
            if (!string.IsNullOrWhiteSpace(sqlUser?.B2CId))
                matchedSqlIds.Add(sqlUser.B2CId);

            normalized.Add(new NormalizedUser
            {
                CosmosId = cosmosId,
                B2CId = b2cId,
                IDS3PublicKey = sqlUser?.PublicKey, // ✅ Set correctly from SqlUser
                Username = sqlUser?.Username,
                DisplayName = cosmosUser?.DisplayName,
                Email = cosmosUser?.Email,
                ClientId = cosmosUser?.ClientId,
                IdpId = cosmosUser?.IdpId,
                B2CIssuer = cosmosUser?.B2CIssuer,
                IDS3Enabled = cosmosUser?.IDS3Enabled ?? false,
                B2CEnabled = cosmosUser?.B2CEnabled ?? false,
                Groups = cosmosUser?.Groups ?? new List<UserGroup>(),
                UserClaims = cosmosUser?.UserClaims ?? new List<UserClaim>(),
                ExistsInCosmos = true,
                ExistsInB2C = b2cUser is not null,
                ExistsInSql = sqlUser is not null
            });
        }

        b2cOrphans = b2cUsers
            .Where(u => u is not null && !string.IsNullOrEmpty(u.Id) && !matchedB2CIds.Contains(u.Id))
            .ToList();

        sqlOrphans = sqlUsers
            .Where(u => u is not null && !string.IsNullOrEmpty(u.B2CId) && !matchedSqlIds.Contains(u.B2CId))
            .ToList();

        return normalized;
    }
}
