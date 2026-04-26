using ExpenseTracker.Application.DTOs;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Interfaces;

namespace ExpenseTracker.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _repository;

    public CategoryService(ICategoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<CategoryDto>> GetAllAsync()
    {
        var categories = await _repository.GetAllAsync();
        return categories.Select(MapToDto);
    }

    public async Task<CategoryDto?> GetByIdAsync(Guid id)
    {
        var category = await _repository.GetByIdAsync(id);
        return category is null ? null : MapToDto(category);
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto)
    {
        ValidateCategoryName(dto.CategoryName);

        var category = new Category
        {
            CategoryName = dto.CategoryName.Trim()
        };

        var created = await _repository.AddAsync(category);
        return MapToDto(created);
    }

    public async Task<CategoryDto?> UpdateAsync(Guid id, UpdateCategoryDto dto)
    {
        ValidateCategoryName(dto.CategoryName);

        var existing = await _repository.GetByIdAsync(id);
        if (existing is null) return null;

        existing.CategoryName = dto.CategoryName.Trim();

        var updated = await _repository.UpdateAsync(existing);
        return updated is null ? null : MapToDto(updated);
    }

    public async Task<bool> DeleteAsync(Guid id) =>
        await _repository.DeleteAsync(id);

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
