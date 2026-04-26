using ExpenseTracker.Application.DTOs;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Interfaces;

namespace ExpenseTracker.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repository;

    public UserService(IUserRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<UserDto>> GetAllAsync()
    {
        var users = await _repository.GetAllAsync();
        return users.Select(MapToDto);
    }

    public async Task<UserDto?> GetByIdAsync(Guid id)
    {
        var user = await _repository.GetByIdAsync(id);
        return user is null ? null : MapToDto(user);
    }

    public async Task<UserDto?> GetByUsernameAsync(string username)
    {
        if (string.IsNullOrWhiteSpace(username)) return null;

        var user = await _repository.GetByUsernameAsync(username.Trim());
        return user is null ? null : MapToDto(user);
    }

    public async Task<UserDto> CreateAsync(CreateUserDto dto)
    {
        ValidateUser(dto.Username, dto.PasswordHash, dto.FirstName, dto.LastName);

        var user = new User
        {
            Username = dto.Username.Trim(),
            PasswordHash = dto.PasswordHash.Trim(),
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim()
        };

        var created = await _repository.AddAsync(user);
        return MapToDto(created);
    }

    public async Task<UserDto?> UpdateAsync(Guid id, UpdateUserDto dto)
    {
        ValidateUser(dto.Username, dto.PasswordHash, dto.FirstName, dto.LastName);

        var existing = await _repository.GetByIdAsync(id);
        if (existing is null) return null;

        existing.Username = dto.Username.Trim();
        existing.PasswordHash = dto.PasswordHash.Trim();
        existing.FirstName = dto.FirstName.Trim();
        existing.LastName = dto.LastName.Trim();

        var updated = await _repository.UpdateAsync(existing);
        return updated is null ? null : MapToDto(updated);
    }

    public async Task<bool> DeleteAsync(Guid id) =>
        await _repository.DeleteAsync(id);

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
