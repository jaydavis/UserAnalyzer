using System.Collections.Generic;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace AnalyzerApp.Models
{
    public class CosmosUser
    {
        [JsonProperty("id")]
        public string? id { get; set; }

        [JsonProperty("B2CId")]
        public string? B2CId { get; set; }

        public string? Username { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }

        public string? ClientId { get; set; }
        public string? IdpId { get; set; }
        public bool IDS3Enabled { get; set; }
        public bool B2CEnabled { get; set; }
        public string? B2CIssuer { get; set; }

        public List<UserGroup>? Groups { get; set; }
        public List<UserClaim>? UserClaims { get; set; }

        public string? DisplayName => 
            string.IsNullOrEmpty(FirstName) && string.IsNullOrEmpty(LastName)
                ? null
                : $"{FirstName} {LastName}".Trim();
    }
}
