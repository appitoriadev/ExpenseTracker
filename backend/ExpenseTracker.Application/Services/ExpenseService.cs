using System.Data.Common;
using ExpenseTracker.Application.DTOs;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace ExpenseTracker.Application.Services;

public class ExpenseService : IExpenseService
{
    private readonly IExpenseRepository _repository;
    private readonly ICategoryService _categoryService;
    private readonly IUserExpenseRepository _userExpenseRepository;
    private readonly ILogger<ExpenseService> _logger;

    public ExpenseService(IExpenseRepository repository, ICategoryService categoryService, IUserExpenseRepository userExpenseRepository, ILogger<ExpenseService> logger)
    {
        _repository = repository;
        _categoryService = categoryService;
        _userExpenseRepository = userExpenseRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<ExpenseResponseDto>> GetAllAsync()
    {
        try
        {
            var expenses = await _repository.GetAllAsync();
            return expenses.Select(MapToResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all expenses");
            throw;
        }
    }

    public async Task<ExpenseResponseDto?> GetByIdAsync(int id)
    {
        try
        {
            var expense = await _repository.GetByIdAsync(id);
            return expense is null ? null : MapToResponse(expense);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving expense with ID {ExpenseId}", id);
            throw;
        }
    }

    public async Task<ExpenseResponseDto> CreateAsync(CreateExpenseDto dto, int userId)
    {
        try
        {
            if (dto is null)
                throw new ArgumentNullException(nameof(dto), "Expense data is required");

            if (string.IsNullOrWhiteSpace(dto.Title))
                throw new ArgumentException("Title is required", nameof(dto.Title));

            if (dto.Amount <= 0)
                throw new ArgumentException("Amount must be greater than zero", nameof(dto.Amount));

            if(string.IsNullOrWhiteSpace(dto.Category))
                throw new ArgumentException("Category is required", nameof(dto.Category));

            var category = await _categoryService.GetByNameAsync(dto.Category);

            if (category is null)
            {
                var categoryDto = new CreateCategoryDto(dto.Category);
                category = await _categoryService.CreateAsync(categoryDto);
            }
            
            var expense = new Expense
            {
                Title    = dto.Title,
                Amount   = dto.Amount,
                CategoryId = category.Id,
                Date     = dto.Date,
                CategoryName = category.CategoryName
            };

            var created = await _repository.AddAsync(expense);

            var userExpense = new UserExpense
            {
                ExpenseId = created.Id,
                UserId = userId
            };
            await _userExpenseRepository.AddAsync(userExpense);

            _logger.LogInformation("Expense created successfully with ID {ExpenseId} for user {UserId}", created.Id, userId);
            return MapToResponse(created);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error while creating expense");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating expense");
            throw;
        }
    }

    public async Task<ExpenseResponseDto?> UpdateAsync(int id, UpdateExpenseDto dto)
    {
        try
        {
            if (dto is null)
                throw new ArgumentNullException(nameof(dto), "Expense data is required");

            var existing = await _repository.GetByIdAsync(id);
            if (existing is null)
            {
                _logger.LogWarning("Attempt to update non-existent expense with ID {ExpenseId}", id);
                return null;
            }

            if (string.IsNullOrWhiteSpace(dto.Title))
                throw new ArgumentException("Title is required", nameof(dto.Title));

            if (dto.Amount <= 0)
                throw new ArgumentException("Amount must be greater than zero", nameof(dto.Amount));

            var category = await _categoryService.GetByNameAsync(dto.Category);

            if (category is null)
                throw new ArgumentException("An error occurred could not find Category", nameof(dto.Category));

            existing.Title    = dto.Title;
            existing.Amount   = dto.Amount;
            existing.CategoryId = category.Id;
            existing.Date     = dto.Date;
            existing.CategoryName = dto.Category;

            var updated = await _repository.UpdateAsync(existing);
            _logger.LogInformation("Expense with ID {ExpenseId} updated successfully", id);
            return updated is null ? null : MapToResponse(updated);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error while updating expense with ID {ExpenseId}", id);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating expense with ID {ExpenseId}", id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var result = await _repository.DeleteAsync(id);
            if (result)
                _logger.LogInformation("Expense with ID {ExpenseId} deleted successfully", id);
            else
                _logger.LogWarning("Attempt to delete non-existent expense with ID {ExpenseId}", id);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting expense with ID {ExpenseId}", id);
            throw;
        }
    }

    private static ExpenseResponseDto MapToResponse(Expense e) =>
        new(e.Id, e.Title, e.Amount, e.CategoryName, e.Date);
}
