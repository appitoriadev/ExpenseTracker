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
            CREATE SCHEMA IF NOT EXISTS dbo;

            CREATE TABLE IF NOT EXISTS dbo.users (
                id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                username VARCHAR(255) NOT NULL UNIQUE,
                password_hash VARCHAR(255) NOT NULL,
                firstname VARCHAR(255) NOT NULL,
                lastname VARCHAR(255) NOT NULL,
                created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
                email VARCHAR(255) UNIQUE,
                refresh_token VARCHAR(512),
                refresh_token_expiry TIMESTAMPTZ
            );

            CREATE TABLE IF NOT EXISTS dbo.categories (
                id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                category_name VARCHAR(255) NOT NULL UNIQUE,
                created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
            );

            CREATE TABLE IF NOT EXISTS dbo.expenses (
                id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                title VARCHAR(255) NOT NULL,
                amount NUMERIC(18, 2) NOT NULL CHECK (amount > 0),
                category_id INT NOT NULL,
                expense_date TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
                created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
                CONSTRAINT fk_categories FOREIGN KEY (category_id) REFERENCES dbo.categories(id)
            );

            CREATE TABLE IF NOT EXISTS dbo.user_expenses (
                id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                expense_id INT NOT NULL,
                user_id INT NOT NULL,
                created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
                CONSTRAINT fk_user FOREIGN KEY (user_id) REFERENCES dbo.users(id) ON DELETE CASCADE,
                CONSTRAINT fk_expense FOREIGN KEY (expense_id) REFERENCES dbo.expenses(id) ON DELETE CASCADE,
                CONSTRAINT uq_user_expense UNIQUE (user_id, expense_id)
            );

            CREATE INDEX IF NOT EXISTS idx_expenses_date ON dbo.expenses(expense_date DESC);
            CREATE INDEX IF NOT EXISTS idx_expenses_category ON dbo.expenses(category_id);
            CREATE INDEX IF NOT EXISTS idx_users_username ON dbo.users(username);
            CREATE INDEX IF NOT EXISTS idx_userexpenses_user ON dbo.user_expenses(user_id);
            CREATE INDEX IF NOT EXISTS idx_userexpenses_expense ON dbo.user_expenses(expense_id);
            """;

        await cmd.ExecuteNonQueryAsync();
    }
}
