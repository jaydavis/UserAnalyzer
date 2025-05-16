namespace AnalyzerApp.Models
{
    public class B2CUser
    {
        public string? Id { get; set; }
        public string? DisplayName { get; set; }
        public string? UserPrincipalName { get; set; }
        public string? GivenName { get; set; }
        public string? Surname { get; set; }
        public string? Email => UserPrincipalName;
    }
}
