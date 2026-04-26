using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Interfaces;
using ExpenseTracker.Infrastructure.Data;
using Npgsql;

namespace ExpenseTracker.Infrastructure.Repositories;

public class ExpenseRepository : IExpenseRepository
{
    private readonly ConnectionProvider _connectionProvider;

    public ExpenseRepository(ConnectionProvider connectionProvider)
    {
        _connectionProvider = connectionProvider;
    }

    public async Task<IEnumerable<Expense>> GetAllAsync()
    {
        var expenses = new List<Expense>();

        using (var connection = _connectionProvider.CreateConnection())
        {
            await connection.OpenAsync();

            using (var command = new NpgsqlCommand(
                "SELECT id, title, amount, category, date FROM expenses ORDER BY date DESC",
                connection))
            {
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        expenses.Add(MapFromReader(reader));
                    }
                }
            }
        }

        return expenses;
    }

    public async Task<Expense?> GetByIdAsync(int id)
    {
        using (var connection = _connectionProvider.CreateConnection())
        {
            await connection.OpenAsync();

            using (var command = new NpgsqlCommand(
                "SELECT id, title, amount, category, date FROM expenses WHERE id = @id",
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

    public async Task<Expense> AddAsync(Expense expense)
    {
        using (var connection = _connectionProvider.CreateConnection())
        {
            await connection.OpenAsync();

            using (var command = new NpgsqlCommand(
                @"INSERT INTO expenses (title, amount, category, date)
                  VALUES (@title, @amount, @category, @date)
                  RETURNING id",
                connection))
            {
                command.Parameters.AddWithValue("@title", expense.Title);
                command.Parameters.AddWithValue("@amount", expense.Amount);
                command.Parameters.AddWithValue("@category", expense.Category);
                command.Parameters.AddWithValue("@date", expense.Date);

                var result = await command.ExecuteScalarAsync();
                expense.Id = (int)(result ?? 0);
            }
        }

        return expense;
    }

    public async Task<Expense?> UpdateAsync(Expense expense)
    {
        using (var connection = _connectionProvider.CreateConnection())
        {
            await connection.OpenAsync();

            using (var command = new NpgsqlCommand(
                @"UPDATE expenses
                  SET title = @title, amount = @amount, category = @category, date = @date
                  WHERE id = @id",
                connection))
            {
                command.Parameters.AddWithValue("@id", expense.Id);
                command.Parameters.AddWithValue("@title", expense.Title);
                command.Parameters.AddWithValue("@amount", expense.Amount);
                command.Parameters.AddWithValue("@category", expense.Category);
                command.Parameters.AddWithValue("@date", expense.Date);

                var rowsAffected = await command.ExecuteNonQueryAsync();
                return rowsAffected > 0 ? expense : null;
            }
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using (var connection = _connectionProvider.CreateConnection())
        {
            await connection.OpenAsync();

            using (var command = new NpgsqlCommand(
                "DELETE FROM expenses WHERE id = @id",
                connection))
            {
                command.Parameters.AddWithValue("@id", id);
                var rowsAffected = await command.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
        }
    }

    private static Expense MapFromReader(NpgsqlDataReader reader) =>
        new()
        {
            Id       = reader.GetInt32(0),
            Title    = reader.GetString(1),
            Amount   = reader.GetDecimal(2),
            Category = reader.GetString(3),
            Date     = reader.GetDateTime(4)
        };
}
