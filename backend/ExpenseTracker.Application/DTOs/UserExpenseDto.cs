namespace ExpenseTracker.Application.DTOs;

public record UserExpenseDto(
    int Id,
    int ExpenseId,
    int UserId,
    DateTime CreatedAt,
    string Username,
    string ExpenseTitle,
    decimal ExpenseAmount,
    DateTime ExpenseDate,
    string CategoryName
);
