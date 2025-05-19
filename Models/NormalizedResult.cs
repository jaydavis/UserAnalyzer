namespace AnalyzerApp.Models;

public class NormalizedResult
{
    public List<NormalizedUser> NormalizedUsers { get; set; } = new();
    public List<B2CUser> B2COrphans { get; set; } = new();
    public List<SqlUser> IDS3Orphans { get; set; } = new();
}

