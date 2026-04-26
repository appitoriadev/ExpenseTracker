using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Interfaces;
using ExpenseTracker.Infrastructure.Data;
using Npgsql;

namespace ExpenseTracker.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ConnectionProvider _connectionProvider;

    public UserRepository(ConnectionProvider connectionProvider)
    {
        _connectionProvider = connectionProvider;
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        var users = new List<User>();

        using (var connection = _connectionProvider.CreateConnection())
        {
            await connection.OpenAsync();

            using (var command = new NpgsqlCommand(
                "SELECT id, username, password_hash, firstname, lastname, created_at FROM dbo.users ORDER BY created_at DESC",
                connection))
            {
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        users.Add(MapFromReader(reader));
                    }
                }
            }
        }

        return users;
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        using (var connection = _connectionProvider.CreateConnection())
        {
            await connection.OpenAsync();

            using (var command = new NpgsqlCommand(
                "SELECT id, username, password_hash, firstname, lastname, created_at FROM dbo.users WHERE id = @id",
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

    public async Task<User?> GetByUsernameAsync(string username)
    {
        using (var connection = _connectionProvider.CreateConnection())
        {
            await connection.OpenAsync();

            using (var command = new NpgsqlCommand(
                "SELECT id, username, password_hash, firstname, lastname, created_at FROM dbo.users WHERE username = @username",
                connection))
            {
                command.Parameters.AddWithValue("@username", username);

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

    public async Task<User> AddAsync(User user)
    {
        using (var connection = _connectionProvider.CreateConnection())
        {
            await connection.OpenAsync();

            using (var command = new NpgsqlCommand(
                @"INSERT INTO dbo.users (username, password_hash, firstname, lastname)
                  VALUES (@username, @passwordHash, @firstName, @lastName)
                  RETURNING id, created_at",
                connection))
            {
                command.Parameters.AddWithValue("@username", user.Username);
                command.Parameters.AddWithValue("@passwordHash", user.PasswordHash);
                command.Parameters.AddWithValue("@firstName", user.FirstName);
                command.Parameters.AddWithValue("@lastName", user.LastName);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        user.Id = reader.GetGuid(0);
                        user.CreatedAt = reader.GetDateTime(1);
                    }
                }
            }
        }

        return user;
    }

    public async Task<User?> UpdateAsync(User user)
    {
        using (var connection = _connectionProvider.CreateConnection())
        {
            await connection.OpenAsync();

            using (var command = new NpgsqlCommand(
                @"UPDATE dbo.users
                  SET username = @username,
                      password_hash = @passwordHash,
                      firstname = @firstName,
                      lastname = @lastName
                  WHERE id = @id",
                connection))
            {
                command.Parameters.AddWithValue("@id", user.Id);
                command.Parameters.AddWithValue("@username", user.Username);
                command.Parameters.AddWithValue("@passwordHash", user.PasswordHash);
                command.Parameters.AddWithValue("@firstName", user.FirstName);
                command.Parameters.AddWithValue("@lastName", user.LastName);

                var rowsAffected = await command.ExecuteNonQueryAsync();
                return rowsAffected > 0 ? user : null;
            }
        }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        using (var connection = _connectionProvider.CreateConnection())
        {
            await connection.OpenAsync();

            using (var command = new NpgsqlCommand(
                "DELETE FROM dbo.users WHERE id = @id",
                connection))
            {
                command.Parameters.AddWithValue("@id", id);
                var rowsAffected = await command.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
        }
    }

    private static User MapFromReader(NpgsqlDataReader reader) =>
        new()
        {
            Id = reader.GetGuid(0),
            Username = reader.GetString(1),
            PasswordHash = reader.GetString(2),
            FirstName = reader.GetString(3),
            LastName = reader.GetString(4),
            CreatedAt = reader.GetDateTime(5)
        };
}
