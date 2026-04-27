using ExpenseTracker.Application.DTOs;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace ExpenseTracker.Application.Services;

public class UserExpenseService : IUserExpenseService
{
    private readonly IUserExpenseRepository _repository;
    private readonly ILogger<UserExpenseService> _logger;

    public UserExpenseService(IUserExpenseRepository repository, ILogger<UserExpenseService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IEnumerable<UserExpenseDto>> GetByUserIdAsync(int userId)
    {
        try
        {
            var userExpenses = await _repository.GetByUserIdAsync(userId);
            return userExpenses.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user expenses for user ID {UserId}", userId);
            throw;
        }
    }

    public async Task<UserExpenseDto?> GetByIdAsync(int id)
    {
        try
        {
            var userExpense = await _repository.GetByIdAsync(id);
            return userExpense is null ? null : MapToDto(userExpense);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user expense with ID {UserExpenseId}", id);
            throw;
        }
    }

    public async Task<UserExpenseDto> CreateAsync(CreateUserExpenseDto dto)
    {
        try
        {
            if (dto is null)
                throw new ArgumentNullException(nameof(dto), "User expense data is required");

            if (dto.ExpenseId <= 0)
                throw new ArgumentException("Expense ID is required", nameof(dto.ExpenseId));

            if (dto.UserId <= 0)
                throw new ArgumentException("User ID is required", nameof(dto.UserId));

            var userExpense = new UserExpense
            {
                ExpenseId = dto.ExpenseId,
                UserId = dto.UserId
            };

            var created = await _repository.AddAsync(userExpense);
            _logger.LogInformation("User expense created successfully for user {UserId} and expense {ExpenseId}", dto.UserId, dto.ExpenseId);
            return MapToDto(created);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error while creating user expense");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user expense");
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var result = await _repository.DeleteAsync(id);
            if (result)
                _logger.LogInformation("User expense with ID {UserExpenseId} deleted successfully", id);
            else
                _logger.LogWarning("Attempt to delete non-existent user expense with ID {UserExpenseId}", id);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user expense with ID {UserExpenseId}", id);
            throw;
        }
    }

    private static UserExpenseDto MapToDto(UserExpense ue) =>
        new(ue.Id, ue.ExpenseId, ue.UserId, ue.CreatedAt,
            ue.Username, ue.ExpenseTitle, ue.ExpenseAmount,
            ue.ExpenseDate, ue.CategoryName);
}
