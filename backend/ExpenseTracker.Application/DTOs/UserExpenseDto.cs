namespace ExpenseTracker.Application.DTOs;

public record UserExpenseDto(
    Guid Id,
    Guid ExpenseId,
    Guid UserId,
    DateTime CreatedAt
);
