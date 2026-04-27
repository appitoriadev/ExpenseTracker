using ExpenseTracker.Application.DTOs;

namespace ExpenseTracker.Application.Interfaces;

public interface IExpenseService
{
    Task<IEnumerable<ExpenseResponseDto>> GetAllAsync();
    Task<ExpenseResponseDto?> GetByIdAsync(Guid id);
    Task<ExpenseResponseDto> CreateAsync(CreateExpenseDto dto);
    Task<ExpenseResponseDto?> UpdateAsync(Guid id, UpdateExpenseDto dto);
    Task<bool> DeleteAsync(Guid id);
}
