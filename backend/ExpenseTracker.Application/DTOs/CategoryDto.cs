namespace ExpenseTracker.Application.DTOs;

public record CategoryDto(
    Guid Id,
    string CategoryName,
    DateTime CreatedAt
);
