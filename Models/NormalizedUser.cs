namespace AnalyzerApp.Models
{
    public class NormalizedUser
    {
        // Cosmos ID (can be Guid.Empty if user isn't in Cosmos)
        public string CosmosId { get; set; } = Guid.Empty.ToString();

        // Profile info
        public string? Email { get; set; }
        public string? Username { get; set; } 
        public string? DisplayName { get; set; }

        // Cosmos metadata
        public string? ClientId { get; set; }
        public string? IdpId { get; set; }
        public bool IDS3Enabled { get; set; }
        public bool B2CEnabled { get; set; }
        public string? B2CIssuer { get; set; }

        // System-specific IDs
        public string? B2CId { get; set; }
        public string? IDS3PublicKey { get; set; }

        // Group and Claims
        public List<UserGroup> Groups { get; set; } = new();
        public List<UserClaim> UserClaims { get; set; } = new();

        // Flags indicating presence
        public bool ExistsInCosmos { get; set; }
        public bool ExistsInB2C { get; set; }
        public bool ExistsInSql { get; set; }

        // Helper for reporting
        public string[] Sources =>
            new[] {
                ExistsInCosmos ? "Cosmos" : null,
                ExistsInB2C ? "B2C" : null,
                ExistsInSql ? "IDS3" : null,
            }.Where(s => s != null).Cast<string>().ToArray();
    }
}