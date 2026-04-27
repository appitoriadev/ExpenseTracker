using ExpenseTracker.Application.DTOs;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace ExpenseTracker.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _repository;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(ICategoryRepository repository, ILogger<CategoryService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IEnumerable<CategoryDto>> GetAllAsync()
    {
        try
        {
            var categories = await _repository.GetAllAsync();
            return categories.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all categories");
            throw;
        }
    }

    public async Task<CategoryDto?> GetByIdAsync(Guid id)
    {
        try
        {
            var category = await _repository.GetByIdAsync(id);
            return category is null ? null : MapToDto(category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving category with ID {CategoryId}", id);
            throw;
        }
    }

    public async Task<CategoryDto?> GetByNameAsync(string name)
    {
        try
        {
            var category = await _repository.GetByNameAsync(name);
            return category is null ? null : MapToDto(category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving category with NAME {name}", name);
            throw;
        }
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto)
    {
        try
        {
            if (dto is null)
                throw new ArgumentNullException(nameof(dto), "Category data is required");

            ValidateCategoryName(dto.CategoryName);

            var category = new Category
            {
                CategoryName = dto.CategoryName.Trim()
            };

            var exists = await _repository.GetByNameAsync(dto.CategoryName);
            if (exists is null)
            {
                var created = await _repository.AddAsync(category);
                _logger.LogInformation("Category created successfully with ID {CategoryId}", created.Id);
                return MapToDto(created);
            }
            return MapToDto(exists);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error while creating category");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating category");
            throw;
        }
    }

    public async Task<CategoryDto?> UpdateAsync(Guid id, UpdateCategoryDto dto)
    {
        try
        {
            if (dto is null)
                throw new ArgumentNullException(nameof(dto), "Category data is required");

            ValidateCategoryName(dto.CategoryName);

            var existing = await _repository.GetByIdAsync(id);
            if (existing is null)
            {
                _logger.LogWarning("Attempt to update non-existent category with ID {CategoryId}", id);
                return null;
            }

            existing.CategoryName = dto.CategoryName.Trim();

            var updated = await _repository.UpdateAsync(existing);
            _logger.LogInformation("Category with ID {CategoryId} updated successfully", id);
            return updated is null ? null : MapToDto(updated);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error while updating category with ID {CategoryId}", id);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating category with ID {CategoryId}", id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            var result = await _repository.DeleteAsync(id);
            if (result)
                _logger.LogInformation("Category with ID {CategoryId} deleted successfully", id);
            else
                _logger.LogWarning("Attempt to delete non-existent category with ID {CategoryId}", id);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting category with ID {CategoryId}", id);
            throw;
        }
    }

    private static CategoryDto MapToDto(Category c) =>
        new(c.Id, c.CategoryName, c.CreatedAt);

    private static void ValidateCategoryName(string categoryName)
    {
        if (string.IsNullOrWhiteSpace(categoryName))
            throw new ArgumentException("Category name is required.", nameof(categoryName));
        if (categoryName.Trim().Length > 255)
            throw new ArgumentException("Category name must be 255 characters or fewer.", nameof(categoryName));
    }
}
