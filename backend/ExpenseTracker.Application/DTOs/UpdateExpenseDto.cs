namespace ExpenseTracker.Application.DTOs;

public record UpdateExpenseDto(
    string Title,
    decimal Amount,
    string Category,
    DateTime Date
);
