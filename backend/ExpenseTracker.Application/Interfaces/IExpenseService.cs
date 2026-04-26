using ExpenseTracker.Application.DTOs;

namespace ExpenseTracker.Application.Interfaces;

public interface IExpenseService
{
    Task<IEnumerable<ExpenseResponseDto>> GetAllAsync();
    Task<ExpenseResponseDto?> GetByIdAsync(int id);
    Task<ExpenseResponseDto> CreateAsync(CreateExpenseDto dto);
    Task<ExpenseResponseDto?> UpdateAsync(int id, UpdateExpenseDto dto);
    Task<bool> DeleteAsync(int id);
}
