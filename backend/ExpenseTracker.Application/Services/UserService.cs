using ExpenseTracker.Application.DTOs;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace ExpenseTracker.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repository;
    private readonly ILogger<UserService> _logger;

    public UserService(IUserRepository repository, ILogger<UserService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IEnumerable<UserDto>> GetAllAsync()
    {
        try
        {
            var users = await _repository.GetAllAsync();
            return users.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all users");
            throw;
        }
    }

    public async Task<UserDto?> GetByIdAsync(int id)
    {
        try
        {
            var user = await _repository.GetByIdAsync(id);
            return user is null ? null : MapToDto(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user with ID {UserId}", id);
            throw;
        }
    }

    public async Task<UserDto?> GetByUsernameAsync(string username)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(username)) return null;

            var user = await _repository.GetByUsernameAsync(username.Trim());
            return user is null ? null : MapToDto(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user with username {Username}", username);
            throw;
        }
    }

    public async Task<UserDto> CreateAsync(CreateUserDto dto)
    {
        try
        {
            if (dto is null)
                throw new ArgumentNullException(nameof(dto), "User data is required");

            ValidateUser(dto.Username, dto.PasswordHash, dto.FirstName, dto.LastName);

            var user = new User
            {
                Username = dto.Username.Trim(),
                PasswordHash = dto.PasswordHash.Trim(),
                FirstName = dto.FirstName.Trim(),
                LastName = dto.LastName.Trim()
            };

            var created = await _repository.AddAsync(user);
            _logger.LogInformation("User {Username} created successfully", created.Username);
            return MapToDto(created);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error while creating user");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            throw;
        }
    }

    public async Task<UserDto?> UpdateAsync(int id, UpdateUserDto dto)
    {
        try
        {
            if (dto is null)
                throw new ArgumentNullException(nameof(dto), "User data is required");

            ValidateUser(dto.Username, dto.PasswordHash, dto.FirstName, dto.LastName);

            var existing = await _repository.GetByIdAsync(id);
            if (existing is null)
            {
                _logger.LogWarning("Attempt to update non-existent user with ID {UserId}", id);
                return null;
            }

            existing.Username = dto.Username.Trim();
            existing.PasswordHash = dto.PasswordHash.Trim();
            existing.FirstName = dto.FirstName.Trim();
            existing.LastName = dto.LastName.Trim();

            var updated = await _repository.UpdateAsync(existing);
            _logger.LogInformation("User with ID {UserId} updated successfully", id);
            return updated is null ? null : MapToDto(updated);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error while updating user with ID {UserId}", id);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user with ID {UserId}", id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var result = await _repository.DeleteAsync(id);
            if (result)
                _logger.LogInformation("User with ID {UserId} deleted successfully", id);
            else
                _logger.LogWarning("Attempt to delete non-existent user with ID {UserId}", id);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user with ID {UserId}", id);
            throw;
        }
    }

    private static UserDto MapToDto(User u) =>
        new(u.Id, u.Username, u.PasswordHash, u.FirstName, u.LastName, u.CreatedAt);

    private static void ValidateUser(string username, string passwordHash, string firstName, string lastName)
    {
        ValidateLength(username, 255, nameof(username));
        ValidateLength(passwordHash, 255, nameof(passwordHash));
        ValidateLength(firstName, 255, nameof(firstName));
        ValidateLength(lastName, 255, nameof(lastName));
    }

    private static void ValidateLength(string value, int max, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{paramName} is required.", paramName);
        if (value.Trim().Length > max)
            throw new ArgumentException($"{paramName} must be {max} characters or fewer.", paramName);
    }
}
