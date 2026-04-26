namespace ExpenseTracker.Application.DTOs;

public record CreateUserDto(
    string Username,
    string PasswordHash,
    string FirstName,
    string LastName
);
