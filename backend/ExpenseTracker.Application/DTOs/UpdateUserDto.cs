namespace ExpenseTracker.Application.DTOs;

public record UpdateUserDto(
    string Username,
    string PasswordHash,
    string FirstName,
    string LastName
);
