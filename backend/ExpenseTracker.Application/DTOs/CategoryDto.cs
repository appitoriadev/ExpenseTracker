namespace ExpenseTracker.Application.DTOs;

public record CategoryDto(
    int Id,
    string CategoryName,
    DateTime CreatedAt
);
