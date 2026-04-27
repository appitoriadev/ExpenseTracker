namespace ExpenseTracker.Application.DTOs;

public record ExpenseResponseDto(
    int Id,
    string Title,
    decimal Amount,
    string Category,
    DateTime Date
);
