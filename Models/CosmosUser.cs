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

        public string? DisplayName{ get
            {
                if (string.IsNullOrEmpty(FirstName) && string.IsNullOrEmpty(LastName))
                    return null;

                return $"{FirstName} {LastName}".Trim();
            }
        }
    }
}
