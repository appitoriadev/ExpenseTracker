using ExpenseTracker.Application.DTOs;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Interfaces;

namespace ExpenseTracker.Application.Services;

public class UserExpenseService : IUserExpenseService
{
    private readonly IUserExpenseRepository _repository;

    public UserExpenseService(IUserExpenseRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<UserExpenseDto>> GetByUserIdAsync(Guid userId)
    {
        var userExpenses = await _repository.GetByUserIdAsync(userId);
        return userExpenses.Select(MapToDto);
    }

    public async Task<UserExpenseDto?> GetByIdAsync(Guid id)
    {
        var userExpense = await _repository.GetByIdAsync(id);
        return userExpense is null ? null : MapToDto(userExpense);
    }

    public async Task<UserExpenseDto> CreateAsync(CreateUserExpenseDto dto)
    {
        var userExpense = new UserExpense
        {
            ExpenseId = dto.ExpenseId,
            UserId = dto.UserId
        };

        var created = await _repository.AddAsync(userExpense);
        return MapToDto(created);
    }

    public async Task<bool> DeleteAsync(Guid id) =>
        await _repository.DeleteAsync(id);

    private static UserExpenseDto MapToDto(UserExpense ue) =>
        new(ue.Id, ue.ExpenseId, ue.UserId, ue.CreatedAt);
}
