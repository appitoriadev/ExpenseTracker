using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Domain.Interfaces;

public interface IUserExpenseRepository
{
    Task<IEnumerable<UserExpense>> GetByUserIdAsync(int userId);
    Task<UserExpense?> GetByIdAsync(int id);
    Task<UserExpense> AddAsync(UserExpense userExpense);
    Task<bool> DeleteAsync(int id);
}
