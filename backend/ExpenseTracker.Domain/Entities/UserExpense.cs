namespace ExpenseTracker.Domain.Entities;

public class UserExpense
{
    public int Id { get; set; }
    public int ExpenseId { get; set; }
    public int UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Username { get; set; } = string.Empty;
    public string ExpenseTitle { get; set; } = string.Empty;
    public decimal ExpenseAmount { get; set; }
    public DateTime ExpenseDate { get; set; }
    public string CategoryName { get; set; } = string.Empty;
}
