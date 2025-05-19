using System;
using System.Linq;
using DotNetEnv;
using AnalyzerApp.Data.Graph;
using AnalyzerApp.Data.Cosmos;
using AnalyzerApp.Data.Sql;
using AnalyzerApp.Data.Secrets;
using AnalyzerApp.Models;
using AnalyzerApp.Services;
using System.Text.Json;

class Program
{
    static async Task Main(string[] args)
    {
        // Load environment variables
        Env.Load();

        string? env = args
            .FirstOrDefault(arg => arg.StartsWith("--env="))
            ?.Split("=")[1]
            ?.ToLower();

        if (string.IsNullOrWhiteSpace(env))
        {
            Console.Write("Enter environment (dev / uat / prod): ");
            env = Console.ReadLine()?.Trim().ToLower();
        }

        if (string.IsNullOrWhiteSpace(env) || !new[] { "dev", "uat", "prod" }.Contains(env))
        {
            Console.WriteLine("❌ Invalid environment. Use 'dev', 'uat', or 'prod'.");
            return;
        }

        var envKey = env.ToUpper();
        Console.WriteLine($"🌐 Environment: {envKey}");

        // ========== Cosmos ==========
        var cosmosUsers = new List<CosmosUser>();

        try
        {
            Console.WriteLine("🔗 Connecting to Cosmos DB...");
            var cosmosConn = Environment.GetEnvironmentVariable($"COSMOS_{env.ToUpper()}_CONNECTION_STRING");
            var cosmosDb = Environment.GetEnvironmentVariable($"COSMOS_{env.ToUpper()}_DATABASE_NAME");
            var cosmosContainer = Environment.GetEnvironmentVariable($"COSMOS_{env.ToUpper()}_CONTAINER_NAME");

            if (string.IsNullOrWhiteSpace(cosmosConn) ||
                string.IsNullOrWhiteSpace(cosmosDb) ||
                string.IsNullOrWhiteSpace(cosmosContainer))
            {
                Console.WriteLine("❌ Cosmos DB environment variables are not set properly.");
                return;
            }

            var cosmosService = new CosmosUserService(cosmosConn, cosmosDb, cosmosContainer);
            cosmosUsers = await cosmosService.GetUsersAsync();

            Console.WriteLine($"✅ Retrieved {cosmosUsers.Count} Cosmos users");
            // foreach (var user in cosmosUsers)
            // {
            //     Console.WriteLine($"*****");
            //     Console.WriteLine($"   id:  {user.id}");
            //     Console.WriteLine($"   First Name: {user.FirstName}");
            //     Console.WriteLine($"   Last Name: {user.LastName}");
            //     Console.WriteLine($"   Email: {user.Email}");
            //     Console.WriteLine($"   B2CID: {user.B2CId}");
            // }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Cosmos DB error: {ex.Message}");
        }

    //     // ========== B2C ==========
        var tenantId = Environment.GetEnvironmentVariable($"B2C_{env.ToUpper()}_TENANT_ID");
        var clientId = Environment.GetEnvironmentVariable($"B2C_{env.ToUpper()}_CLIENT_ID");
        var clientSecret = Environment.GetEnvironmentVariable($"B2C_{env.ToUpper()}_CLIENT_SECRET");

        if (string.IsNullOrWhiteSpace(tenantId) ||
            string.IsNullOrWhiteSpace(clientId) ||
            string.IsNullOrWhiteSpace(clientSecret))
        {
            Console.WriteLine("❌ B2C environment variables are not set properly.");
            return;
        }

        var b2cUsers = new List<B2CUser>();

        try
        {
            Console.WriteLine("🔗 Connecting to Microsoft Graph (B2C)...");
            var graphClient = GraphClientFactory.Create(tenantId, clientId, clientSecret);
            var b2cService = new B2CUserService(graphClient);

            var rawB2CUsers = await b2cService.GetAllUsersAsync();

            b2cUsers = rawB2CUsers
                .Select(u => new B2CUser
                {
                    Id = u.Id,
                    DisplayName = u.DisplayName,
                    UserPrincipalName = u.UserPrincipalName,
                    GivenName = u.GivenName,
                    Surname = u.Surname
                })
                .ToList();

            Console.WriteLine($"✅ Retrieved {b2cUsers.Count} B2C users");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ B2C error: {ex.Message}");
        }

        // ========== SQL ==========
        var sqlUsers = new List<SqlUser>();

        try
        {
            Console.WriteLine("🔗 Connecting to Azure SQL...");
            var vaultName = Environment.GetEnvironmentVariable($"KEYVAULT_{env.ToUpper()}_NAME");
            var sqlServer = Environment.GetEnvironmentVariable($"SQL_{env.ToUpper()}_SERVER");
            var sqlDb = Environment.GetEnvironmentVariable($"SQL_DB");
            var userSecretName = Environment.GetEnvironmentVariable("SQL_USER_SECRET");
            var passSecretName = Environment.GetEnvironmentVariable("SQL_PASS_SECRET");

            if (string.IsNullOrWhiteSpace(vaultName) ||
                string.IsNullOrWhiteSpace(sqlServer) ||
                string.IsNullOrWhiteSpace(sqlDb) ||
                string.IsNullOrWhiteSpace(userSecretName) ||
                string.IsNullOrWhiteSpace(passSecretName))
            {
                Console.WriteLine("❌ SQL environment variables are not set properly.");
                return;
            }
            
            var sqlUser = await KeyVaultHelper.GetSecretAsync(vaultName, userSecretName);
            var sqlPass = await KeyVaultHelper.GetSecretAsync(vaultName, passSecretName);

            var connection = $"Server=tcp:{sqlServer},1433;" +
                             $"Initial Catalog={sqlDb};" +
                             $"Persist Security Info=False;" +
                             $"User ID={sqlUser};" +
                             $"Password={sqlPass};" +
                             $"MultipleActiveResultSets=False;" +
                             $"Encrypt=True;" +
                             $"TrustServerCertificate=False;" +
                             $"Connection Timeout=30;";

            var sqlService = new SqlUserService(connection);
            sqlUsers = await sqlService.GetUsersAsync();

            Console.WriteLine($"✅ Retrieved {sqlUsers.Count} SQL users");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ SQL error: {ex.Message}");
        }

        // ========== Normalize + Compare ==========
        Console.WriteLine("\n🔍 Normalizing users across sources...");

        var normalizedUsers = UserNormalizerService.NormalizeUsers(
            b2cUsers, cosmosUsers, sqlUsers,
            out var b2cOrphans, out var sqlOrphans);


        Console.WriteLine($"✅ Normalized {normalizedUsers.Count} unique users\n");
        var result = new NormalizedResult
        {
            NormalizedUsers = normalizedUsers,
            B2COrphans = b2cOrphans,
            IDS3Orphans = sqlOrphans
        };

        var jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var outputPath = $"output/normalized-output-{env}.json";
        var json = JsonSerializer.Serialize(result, jsonOptions);
        await File.WriteAllTextAsync(outputPath, json);

        Console.WriteLine($"📄 Output written to {outputPath}");


        // foreach (var user in normalizedUsers.Take(5))
        foreach (var user in normalizedUsers.Take(5))
        {
            Console.WriteLine($"🔑   CosmosId: {user.CosmosId}");
            Console.WriteLine($"🔑   B2CId: {user.B2CId}");
            Console.WriteLine($"     Email: {user.Email}");
            Console.WriteLine($"     Username: {user.Username}");
            Console.WriteLine($"     Display Name: {user.DisplayName}");
            Console.WriteLine($"     Sources: {string.Join(", ", user.Sources)}\n");
        }
     }
}
