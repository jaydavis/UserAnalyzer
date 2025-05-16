using Microsoft.Data.SqlClient;
using AnalyzerApp.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AnalyzerApp.Data.Sql
{
    public class SqlUserService
    {
        private readonly string _connectionString;

        public SqlUserService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<List<SqlUser>> GetUsersAsync()
        {
            var users = new List<SqlUser>();
            var query = "SELECT PublicKey, Username FROM Users WHERE Enabled = 1";

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                users.Add(new SqlUser
                {
                    PublicKey = reader["PublicKey"]?.ToString(),
                    Username = reader["Username"]?.ToString()
                });
            }

            return users;
        }    
    }
}
