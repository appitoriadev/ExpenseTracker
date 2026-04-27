using Npgsql;
using Testcontainers.PostgreSql;

namespace ExpenseTracker.Tests.Fixtures;

public class DatabaseFixture : IAsyncLifetime
{
    private PostgreSqlContainer _container = null!;
    private NpgsqlConnection _connection = null!;

    public string ConnectionString { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        _container = new PostgreSqlBuilder("postgres:17-alpine")
            .Build();

        await _container.StartAsync();

        ConnectionString = _container.GetConnectionString();
        _connection = new NpgsqlConnection(ConnectionString);
        await _connection.OpenAsync();

        await InitializeDatabaseSchema();
    }

    public async Task DisposeAsync()
    {
        if (_connection.State == System.Data.ConnectionState.Open)
        {
            await _connection.CloseAsync();
        }

        await _container.StopAsync();
        await _container.DisposeAsync();
    }

    private async Task InitializeDatabaseSchema()
    {
        using var cmd = _connection.CreateCommand();

        cmd.CommandText = """
            CREATE TABLE IF NOT EXISTS users (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                username VARCHAR(255) NOT NULL UNIQUE,
                password_hash VARCHAR(255) NOT NULL,
                firstname VARCHAR(255) NOT NULL,
                lastname VARCHAR(255) NOT NULL,
                created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                email VARCHAR(255) UNIQUE,
                refresh_token VARCHAR(512),
                refresh_token_expiry TIMESTAMP
            );

            CREATE TABLE IF NOT EXISTS categories (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                category_name VARCHAR(255) NOT NULL UNIQUE,
                created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
            );

            CREATE TABLE IF NOT EXISTS expenses (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                title VARCHAR(255) NOT NULL,
                amount NUMERIC(18, 2) NOT NULL CHECK (amount > 0),
                category_name VARCHAR(255) NOT NULL,
                date TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                CONSTRAINT fk_categories FOREIGN KEY (category_name) REFERENCES categories(category_name)
            );

            CREATE TABLE IF NOT EXISTS user_expenses (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
                expenses_id UUID NOT NULL REFERENCES expenses(id) ON DELETE CASCADE,
                created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
            );

            CREATE INDEX IF NOT EXISTS idx_categories ON categories(id DESC);
            CREATE INDEX IF NOT EXISTS idx_expenses_date ON expenses(date DESC);
            CREATE INDEX IF NOT EXISTS idx_expenses_category ON expenses(category_name DESC);
            CREATE INDEX IF NOT EXISTS idx_users_username ON users(username);
            CREATE INDEX IF NOT EXISTS idx_userexpenses_ids ON user_expenses(user_id);
            CREATE INDEX IF NOT EXISTS idx_expenses_ids ON user_expenses(expenses_id);
            """;

        await cmd.ExecuteNonQueryAsync();
    }
}
