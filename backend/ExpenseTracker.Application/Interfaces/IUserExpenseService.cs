using ExpenseTracker.Application.DTOs;

namespace ExpenseTracker.Application.Interfaces;

public interface IUserExpenseService
{
    Task<IEnumerable<UserExpenseDto>> GetByUserIdAsync(Guid userId);
    Task<UserExpenseDto?> GetByIdAsync(Guid id);
    Task<UserExpenseDto> CreateAsync(CreateUserExpenseDto dto);
    Task<bool> DeleteAsync(Guid id);
}
