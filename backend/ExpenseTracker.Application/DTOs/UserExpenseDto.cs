namespace ExpenseTracker.Application.DTOs;

public record UserExpenseDto(
    int Id,
    int ExpenseId,
    int UserId,
    DateTime CreatedAt
);
