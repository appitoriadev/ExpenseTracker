using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Interfaces;
using ExpenseTracker.Infrastructure.Data;
using Npgsql;

namespace ExpenseTracker.Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly ConnectionProvider _connectionProvider;

    public CategoryRepository(ConnectionProvider connectionProvider)
    {
        _connectionProvider = connectionProvider;
    }

    public async Task<IEnumerable<Category>> GetAllAsync()
    {
        var categories = new List<Category>();

        using (var connection = _connectionProvider.CreateConnection())
        {
            await connection.OpenAsync();

            using (var command = new NpgsqlCommand(
                "SELECT id, category_name, created_at FROM dbo.categories ORDER BY created_at DESC",
                connection))
            {
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        categories.Add(MapFromReader(reader));
                    }
                }
            }
        }

        return categories;
    }

    public async Task<Category?> GetByIdAsync(Guid id)
    {
        using (var connection = _connectionProvider.CreateConnection())
        {
            await connection.OpenAsync();

            using (var command = new NpgsqlCommand(
                "SELECT id, category_name, created_at FROM dbo.categories WHERE id = @id",
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

    public async Task<Category> AddAsync(Category category)
    {
        using (var connection = _connectionProvider.CreateConnection())
        {
            await connection.OpenAsync();

            using (var command = new NpgsqlCommand(
                @"INSERT INTO dbo.categories (category_name)
                  VALUES (@categoryName)
                  RETURNING id, created_at",
                connection))
            {
                command.Parameters.AddWithValue("@categoryName", category.CategoryName);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        category.Id = reader.GetGuid(0);
                        category.CreatedAt = reader.GetDateTime(1);
                    }
                }
            }
        }

        return category;
    }

    public async Task<Category?> UpdateAsync(Category category)
    {
        using (var connection = _connectionProvider.CreateConnection())
        {
            await connection.OpenAsync();

            using (var command = new NpgsqlCommand(
                @"UPDATE dbo.categories
                  SET category_name = @categoryName
                  WHERE id = @id",
                connection))
            {
                command.Parameters.AddWithValue("@id", category.Id);
                command.Parameters.AddWithValue("@categoryName", category.CategoryName);

                var rowsAffected = await command.ExecuteNonQueryAsync();
                return rowsAffected > 0 ? category : null;
            }
        }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        using (var connection = _connectionProvider.CreateConnection())
        {
            await connection.OpenAsync();

            using (var command = new NpgsqlCommand(
                "DELETE FROM dbo.categories WHERE id = @id",
                connection))
            {
                command.Parameters.AddWithValue("@id", id);
                var rowsAffected = await command.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
        }
    }

    private static Category MapFromReader(NpgsqlDataReader reader) =>
        new()
        {
            Id = reader.GetGuid(0),
            CategoryName = reader.GetString(1),
            CreatedAt = reader.GetDateTime(2)
        };
}
