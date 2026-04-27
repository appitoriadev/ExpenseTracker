namespace ExpenseTracker.Domain.Entities;

public class UserExpense
{
    public int Id { get; set; }
    public int ExpenseId { get; set; }
    public int UserId { get; set; }
    public DateTime CreatedAt { get; set; }
}
