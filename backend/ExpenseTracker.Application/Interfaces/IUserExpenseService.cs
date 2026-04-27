using ExpenseTracker.Application.DTOs;

namespace ExpenseTracker.Application.Interfaces;

public interface IUserExpenseService
{
    Task<IEnumerable<UserExpenseDto>> GetByUserIdAsync(int userId);
    Task<UserExpenseDto?> GetByIdAsync(int id);
    Task<UserExpenseDto> CreateAsync(CreateUserExpenseDto dto);
    Task<bool> DeleteAsync(int id);
}
