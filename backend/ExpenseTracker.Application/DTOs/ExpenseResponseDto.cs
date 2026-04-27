namespace ExpenseTracker.Application.DTOs;

public record ExpenseResponseDto(
    Guid Id,
    string Title,
    decimal Amount,
    string Category,
    DateTime Date
);
