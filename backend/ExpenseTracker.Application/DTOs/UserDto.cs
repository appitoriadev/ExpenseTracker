namespace ExpenseTracker.Application.DTOs;

public record UserDto(
	Guid Id,
	string Username,
	string PasswordHash,
	string FirstName,
	string LastName,
	DateTime CreatedAt
);
