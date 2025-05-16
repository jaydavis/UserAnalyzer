namespace AnalyzerApp.Models
{
    public class NormalizedUser
    {
        public string CommonId { get; set; } = "";   // The shared ID (e.g. PublicKey/UserId/GraphId)
        public string? Email { get; set; }
        public string? Username { get; set; }
        public string? DisplayName { get; set; }

        // Source-specific metadata (optional)
        public bool ExistsInB2C { get; set; }
        public bool ExistsInCosmos { get; set; }
        public bool ExistsInSql { get; set; }

        public string[] Sources
        {
            get
            {
                var sources = new List<string>();
                if (ExistsInB2C) sources.Add("B2C");
                if (ExistsInCosmos) sources.Add("Cosmos");
                if (ExistsInSql) sources.Add("SQL");
                return sources.ToArray();
            }
        }
    }
}
