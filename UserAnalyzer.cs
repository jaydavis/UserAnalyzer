using AnalyzerApp.Models;
using System.Collections.Generic;

namespace AnalyzerApp.Services
{
    public class UserAnalyzer
    {
        public List<UserRecord> DevUsers { get; set; } = new();
        public List<UserRecord> UatUsers { get; set; } = new();
        public List<UserRecord> ProdUsers { get; set; } = new();

        public void Compare()
        {
            // Later: implement logic to diff across environments
            Console.WriteLine($"Loaded {DevUsers.Count} users from Dev.");
            Console.WriteLine($"Loaded {UatUsers.Count} users from UAT.");
            Console.WriteLine($"Loaded {ProdUsers.Count} users from Prod.");
        }
    }
}
