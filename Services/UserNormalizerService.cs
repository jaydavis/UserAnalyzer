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

        var matchedB2CIds = new HashSet<string>();
        var matchedSqlIds = new HashSet<string>();

        foreach (var cosmosUser in cosmosUsers)
        {
            var cosmosId = cosmosUser.id;
            var b2cId = cosmosUser.B2CId;

            var b2cUser = b2cUsers.FirstOrDefault(u => u.Id == b2cId);
            var sqlUser = sqlUsers.FirstOrDefault(u => u.B2CId == b2cId);

            if (b2cUser != null && b2cUser.Id != null)
            {
                matchedB2CIds.Add(b2cUser.Id);
            }

            if (sqlUser != null && sqlUser.B2CId != null)
            {
                matchedSqlIds.Add(sqlUser.B2CId);
            }

            normalized.Add(new NormalizedUser
            {
                CosmosId = cosmosId ?? string.Empty,
                B2CId = b2cId,
                IDS3PublicKey = sqlUser?.PublicKey,
                Email = cosmosUser.Email,
                Username = sqlUser?.Username,
                DisplayName = cosmosUser.DisplayName,
                ExistsInCosmos = true,
                ExistsInB2C = b2cUser != null,
                ExistsInSql = sqlUser != null
            });
        }

        b2cOrphans = b2cUsers
            .Where(u => u.Id != null && !matchedB2CIds.Contains(u.Id))
            .ToList();

        sqlOrphans = sqlUsers
            .Where(u => u.B2CId != null && !matchedSqlIds.Contains(u.B2CId))
            .ToList();

        return normalized;
    }
}
