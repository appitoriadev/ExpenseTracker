namespace ExpenseTracker.Application.DTOs;

public record CreateUserExpenseDto(
    Guid ExpenseId,
    Guid UserId
);
