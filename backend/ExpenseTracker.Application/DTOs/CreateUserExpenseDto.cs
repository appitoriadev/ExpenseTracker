namespace ExpenseTracker.Application.DTOs;

public record CreateUserExpenseDto(
    int ExpenseId,
    int UserId
);
