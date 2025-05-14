namespace AnalyzerApp.Models
{
    public class UserRecord
    {
        public string? ObjectId { get; set; }         // B2C ID or AAD ID
        public string? CosmosId { get; set; }         // Cosmos DB _id
        public string? SqlId { get; set; }            // SQL ID (if applicable)

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }

        public bool IsInB2C => !string.IsNullOrWhiteSpace(ObjectId);
        public bool IsInCosmos => !string.IsNullOrWhiteSpace(CosmosId);
        public bool IsInSql => !string.IsNullOrWhiteSpace(SqlId);

        public override string ToString()
        {
            return $"{FirstName} {LastName} ({Email})";
        }
    }
}
