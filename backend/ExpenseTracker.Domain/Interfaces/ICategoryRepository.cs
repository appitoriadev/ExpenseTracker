using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Domain.Interfaces;

public interface ICategoryRepository
{
    Task<IEnumerable<Category>> GetAllAsync();
    Task<Category?> GetByIdAsync(Guid id);
    Task<Category?> GetByNameAsync(string name);
    Task<Category> AddAsync(Category category);
    Task<Category?> UpdateAsync(Category category);
    Task<bool> DeleteAsync(Guid id);
}
