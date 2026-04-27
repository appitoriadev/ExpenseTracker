using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Interfaces;
using ExpenseTracker.Infrastructure.Data;
using Npgsql;

namespace ExpenseTracker.Infrastructure.Repositories;

public class UserExpenseRepository : IUserExpenseRepository
{
    private readonly ConnectionProvider _connectionProvider;

    public UserExpenseRepository(ConnectionProvider connectionProvider)
    {
        _connectionProvider = connectionProvider;
    }

    public async Task<IEnumerable<UserExpense>> GetByUserIdAsync(int userId)
    {
        var userExpenses = new List<UserExpense>();

        using (var connection = _connectionProvider.CreateConnection())
        {
            await connection.OpenAsync();

            using (var command = new NpgsqlCommand(
                @"SELECT id, expenses_id, user_id, created_at
                  FROM dbo.user_expenses
                  WHERE user_id = @userId
                  ORDER BY created_at DESC",
                connection))
            {
                command.Parameters.AddWithValue("@userId", userId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        userExpenses.Add(MapFromReader(reader));
                    }
                }
            }
        }

        return userExpenses;
    }

    public async Task<UserExpense?> GetByIdAsync(int id)
    {
        using (var connection = _connectionProvider.CreateConnection())
        {
            await connection.OpenAsync();

            using (var command = new NpgsqlCommand(
                "SELECT id, expenses_id, user_id, created_at FROM dbo.user_expenses WHERE id = @id",
                connection))
            {
                command.Parameters.AddWithValue("@id", id);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return MapFromReader(reader);
                    }
                }
            }
        }

        return null;
    }

    public async Task<UserExpense> AddAsync(UserExpense userExpense)
    {
        using (var connection = _connectionProvider.CreateConnection())
        {
            await connection.OpenAsync();

            using (var command = new NpgsqlCommand(
                @"INSERT INTO dbo.user_expenses (expenses_id, user_id)
                  VALUES (@expenseId, @userId)
                  RETURNING id, created_at",
                connection))
            {
                command.Parameters.AddWithValue("@expenseId", userExpense.ExpenseId);
                command.Parameters.AddWithValue("@userId", userExpense.UserId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        userExpense.Id = reader.GetInt32(0);
                        userExpense.CreatedAt = reader.GetDateTime(1);
                    }
                }
            }
        }

        return userExpense;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using (var connection = _connectionProvider.CreateConnection())
        {
            await connection.OpenAsync();

            using (var command = new NpgsqlCommand(
                "DELETE FROM dbo.user_expenses WHERE id = @id",
                connection))
            {
                command.Parameters.AddWithValue("@id", id);
                var rowsAffected = await command.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
        }
    }

    private static UserExpense MapFromReader(NpgsqlDataReader reader) =>
        new()
        {
            Id = reader.GetInt32(0),
            ExpenseId = reader.GetInt32(1),
            UserId = reader.GetInt32(2),
            CreatedAt = reader.GetDateTime(3)
        };
}
