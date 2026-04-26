using Microsoft.Extensions.Configuration;
using Npgsql;

namespace ExpenseTracker.Infrastructure.Data;

public class ConnectionProvider
{
    private readonly string _connectionString;
    private static bool _schemaInitialized = false;

    public ConnectionProvider(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string cannot be null or empty.", nameof(connectionString));

        _connectionString = connectionString;
    }

    public async Task EnsureSchemaAsync(string path)
    {
        if (_schemaInitialized) return;

        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            // Create tables if they don't exist
            var schema = System.IO.File.ReadAllText(path); 
            Console.Write(schema);

            using (var command = new NpgsqlCommand(schema, connection))
            {
                await command.ExecuteNonQueryAsync();
            }

            _schemaInitialized = true;
        }
    }

    public NpgsqlConnection CreateConnection() =>
        new(_connectionString);
}
