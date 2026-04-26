namespace ExpenseTracker.Application.DTOs;

public record CreateExpenseDto(
    string Title,
    decimal Amount,
    string Category,
    DateTime Date
);
