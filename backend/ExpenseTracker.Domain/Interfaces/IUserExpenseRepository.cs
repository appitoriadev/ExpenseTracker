using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Domain.Interfaces;

public interface IUserExpenseRepository
{
    Task<IEnumerable<UserExpense>> GetByUserIdAsync(Guid userId);
    Task<UserExpense?> GetByIdAsync(Guid id);
    Task<UserExpense> AddAsync(UserExpense userExpense);
    Task<bool> DeleteAsync(Guid id);
}
