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

        // System-specific IDs
        public string? B2CId { get; set; }
        public string? IDS3PublicKey { get; set; }

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