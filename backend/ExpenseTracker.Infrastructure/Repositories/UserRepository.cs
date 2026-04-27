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
                "SELECT id, username, password_hash, firstname, lastname, created_at, email, refresh_token, refresh_token_expiry FROM dbo.users ORDER BY created_at DESC",
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

    public async Task<User?> GetByIdAsync(int id)
    {
        using (var connection = _connectionProvider.CreateConnection())
        {
            await connection.OpenAsync();

            using (var command = new NpgsqlCommand(
                "SELECT id, username, password_hash, firstname, lastname, created_at, email, refresh_token, refresh_token_expiry FROM dbo.users WHERE id = @id",
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
                "SELECT id, username, password_hash, firstname, lastname, created_at, email, refresh_token, refresh_token_expiry FROM dbo.users WHERE username = @username",
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

    public async Task<User?> GetByEmailAsync(string email)
    {
        using (var connection = _connectionProvider.CreateConnection())
        {
            await connection.OpenAsync();

            using (var command = new NpgsqlCommand(
                "SELECT id, username, password_hash, firstname, lastname, created_at, email, refresh_token, refresh_token_expiry FROM dbo.users WHERE email = @email",
                connection))
            {
                command.Parameters.AddWithValue("@email", email);

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
                @"INSERT INTO dbo.users (username, password_hash, firstname, lastname, email, refresh_token, refresh_token_expiry)
                  VALUES (@username, @passwordHash, @firstName, @lastName, @email, @refreshToken, @refreshTokenExpiry)
                  RETURNING id, created_at",
                connection))
            {
                command.Parameters.AddWithValue("@username", user.Username);
                command.Parameters.AddWithValue("@passwordHash", user.PasswordHash);
                command.Parameters.AddWithValue("@firstName", user.FirstName);
                command.Parameters.AddWithValue("@lastName", user.LastName);
                command.Parameters.AddWithValue("@email", (object?)user.Email ?? DBNull.Value);
                command.Parameters.AddWithValue("@refreshToken", (object?)user.RefreshToken ?? DBNull.Value);
                command.Parameters.AddWithValue("@refreshTokenExpiry", (object?)user.RefreshTokenExpiry ?? DBNull.Value);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        user.Id = reader.GetInt32(0);
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
                      lastname = @lastName,
                      email = @email,
                      refresh_token = @refreshToken,
                      refresh_token_expiry = @refreshTokenExpiry
                  WHERE id = @id",
                connection))
            {
                command.Parameters.AddWithValue("@id", user.Id);
                command.Parameters.AddWithValue("@username", user.Username);
                command.Parameters.AddWithValue("@passwordHash", user.PasswordHash);
                command.Parameters.AddWithValue("@firstName", user.FirstName);
                command.Parameters.AddWithValue("@lastName", user.LastName);
                command.Parameters.AddWithValue("@email", (object?)user.Email ?? DBNull.Value);
                command.Parameters.AddWithValue("@refreshToken", (object?)user.RefreshToken ?? DBNull.Value);
                command.Parameters.AddWithValue("@refreshTokenExpiry", (object?)user.RefreshTokenExpiry ?? DBNull.Value);

                var rowsAffected = await command.ExecuteNonQueryAsync();
                return rowsAffected > 0 ? user : null;
            }
        }
    }

    public async Task<bool> DeleteAsync(int id)
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
            Id = reader.GetInt32(0),
            Username = reader.GetString(1),
            PasswordHash = reader.GetString(2),
            FirstName = reader.GetString(3),
            LastName = reader.GetString(4),
            CreatedAt = reader.GetDateTime(5),
            Email = reader.IsDBNull(6) ? null : reader.GetString(6),
            RefreshToken = reader.IsDBNull(7) ? null : reader.GetString(7),
            RefreshTokenExpiry = reader.IsDBNull(8) ? null : reader.GetDateTime(8)
        };
}
