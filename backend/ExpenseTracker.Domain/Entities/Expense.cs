namespace ExpenseTracker.Domain.Entities;

public class Expense
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
}
