namespace ExpenseTracker.Domain.Entities;

public class UserExpense
{
    public Guid Id { get; set; }
    public Guid ExpenseId { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreatedAt { get; set; }
}
