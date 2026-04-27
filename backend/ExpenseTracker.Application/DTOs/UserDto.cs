namespace ExpenseTracker.Application.DTOs;

public record UserDto(
	int Id,
	string Username,
	string PasswordHash,
	string FirstName,
	string LastName,
	DateTime CreatedAt
);
