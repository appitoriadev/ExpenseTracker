using ExpenseTracker.Application.DTOs;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Interfaces;

namespace ExpenseTracker.Application.Services;

public class ExpenseService : IExpenseService
{
    private readonly IExpenseRepository _repository;

    public ExpenseService(IExpenseRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<ExpenseResponseDto>> GetAllAsync()
    {
        var expenses = await _repository.GetAllAsync();
        return expenses.Select(MapToResponse);
    }

    public async Task<ExpenseResponseDto?> GetByIdAsync(int id)
    {
        var expense = await _repository.GetByIdAsync(id);
        return expense is null ? null : MapToResponse(expense);
    }

    public async Task<ExpenseResponseDto> CreateAsync(CreateExpenseDto dto)
    {
        var expense = new Expense
        {
            Title    = dto.Title,
            Amount   = dto.Amount,
            Category = dto.Category,
            Date     = dto.Date
        };

        var created = await _repository.AddAsync(expense);
        return MapToResponse(created);
    }

    public async Task<ExpenseResponseDto?> UpdateAsync(int id, UpdateExpenseDto dto)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing is null) return null;

        existing.Title    = dto.Title;
        existing.Amount   = dto.Amount;
        existing.Category = dto.Category;
        existing.Date     = dto.Date;

        var updated = await _repository.UpdateAsync(existing);
        return updated is null ? null : MapToResponse(updated);
    }

    public async Task<bool> DeleteAsync(int id) =>
        await _repository.DeleteAsync(id);

    private static ExpenseResponseDto MapToResponse(Expense e) =>
        new(e.Id, e.Title, e.Amount, e.Category, e.Date);
}
