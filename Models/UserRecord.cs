namespace AnalyzerApp.Models;

public class UserRecord
{
    public string? Email { get; set; }
    public string? DisplayName { get; set; }
    public string? Username { get; set; }

    public CosmosUser? SotRecord { get; set; }
    public B2CUser? B2CRecord { get; set; }
    public SqlUser? IDS3Record { get; set; }

    public string CommonId =>
        Email ??
        Username ??
        B2CRecord?.UserPrincipalName ??
        SotRecord?.Email ??
        "Unknown";

    public List<string> Sources
    {
        get
        {
            var sources = new List<string>();
            if (SotRecord != null) sources.Add("SoT");
            if (B2CRecord != null) sources.Add("B2C");
            if (IDS3Record != null) sources.Add("IDS3");
            return sources;
        }
    }
}
