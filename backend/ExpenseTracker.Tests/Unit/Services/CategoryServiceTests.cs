using ExpenseTracker.Application.DTOs;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Application.Services;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace ExpenseTracker.Tests.Unit.Services;

public class CategoryServiceTests
{
    private readonly Mock<ICategoryRepository> _mockRepository;
    private readonly Mock<ILogger<CategoryService>> _mockLogger;
    private readonly ICategoryService _service;

    public CategoryServiceTests()
    {
        _mockRepository = new Mock<ICategoryRepository>();
        _mockLogger = new Mock<ILogger<CategoryService>>();
        _service = new CategoryService(_mockRepository.Object, _mockLogger.Object);
    }

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_WithCategories_ReturnsAllCategories()
    {
        var categories = new List<Category>
        {
            new() { Id = 1, CategoryName = "Food",          CreatedAt = DateTime.Now },
            new() { Id = 2, CategoryName = "Transportation", CreatedAt = DateTime.Now }
        };

        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(categories);

        var result = await _service.GetAllAsync();

        result.Should().HaveCount(2);
        result.First().CategoryName.Should().Be("Food");
        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_WithNoCategories_ReturnsEmptyCollection()
    {
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Category>());

        var result = await _service.GetAllAsync();

        result.Should().BeEmpty();
        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsCategory()
    {
        var id = 1;
        var category = new Category { Id = id, CategoryName = "Food", CreatedAt = DateTime.Now };

        _mockRepository.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(category);

        var result = await _service.GetByIdAsync(id);

        result.Should().NotBeNull();
        result!.CategoryName.Should().Be("Food");
        _mockRepository.Verify(r => r.GetByIdAsync(id), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ReturnsNull()
    {
        var id = 999;
        _mockRepository.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Category?)null);

        var result = await _service.GetByIdAsync(id);

        result.Should().BeNull();
        _mockRepository.Verify(r => r.GetByIdAsync(id), Times.Once);
    }

    #endregion

    #region GetByNameAsync Tests

    [Fact]
    public async Task GetByNameAsync_WithValidName_ReturnsCategory()
    {
        var category = new Category { Id = 3, CategoryName = "Health", CreatedAt = DateTime.Now };

        _mockRepository.Setup(r => r.GetByNameAsync("Health")).ReturnsAsync(category);

        var result = await _service.GetByNameAsync("Health");

        result.Should().NotBeNull();
        result!.CategoryName.Should().Be("Health");
        _mockRepository.Verify(r => r.GetByNameAsync("Health"), Times.Once);
    }

    [Fact]
    public async Task GetByNameAsync_WithInvalidName_ReturnsNull()
    {
        _mockRepository.Setup(r => r.GetByNameAsync("NonExistent")).ReturnsAsync((Category?)null);

        var result = await _service.GetByNameAsync("NonExistent");

        result.Should().BeNull();
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_WithValidDto_CreatesAndReturnsCategory()
    {
        var dto = new CreateCategoryDto("Shopping");
        var created = new Category { Id = 5, CategoryName = "Shopping", CreatedAt = DateTime.Now };

        _mockRepository.Setup(r => r.GetByNameAsync("Shopping")).ReturnsAsync((Category?)null);
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Category>())).ReturnsAsync(created);

        var result = await _service.CreateAsync(dto);

        result.Should().NotBeNull();
        result.CategoryName.Should().Be("Shopping");
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Category>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithExistingName_ReturnsExistingCategory()
    {
        var dto = new CreateCategoryDto("Food");
        var existing = new Category { Id = 1, CategoryName = "Food", CreatedAt = DateTime.Now };

        _mockRepository.Setup(r => r.GetByNameAsync("Food")).ReturnsAsync(existing);

        var result = await _service.CreateAsync(dto);

        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Category>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WithEmptyName_ThrowsArgumentException()
    {
        var dto = new CreateCategoryDto("");

        var act = async () => await _service.CreateAsync(dto);

        await act.Should().ThrowAsync<ArgumentException>();
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Category>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WithNameExceedingLimit_ThrowsArgumentException()
    {
        var longName = new string('A', 256);
        var dto = new CreateCategoryDto(longName);

        var act = async () => await _service.CreateAsync(dto);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WithValidId_UpdatesAndReturnsCategory()
    {
        var id = 10;
        var existing = new Category { Id = id, CategoryName = "Old", CreatedAt = DateTime.Now };
        var updated  = new Category { Id = id, CategoryName = "New", CreatedAt = DateTime.Now };
        var dto = new UpdateCategoryDto("New");

        _mockRepository.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(existing);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Category>())).ReturnsAsync(updated);

        var result = await _service.UpdateAsync(id, dto);

        result.Should().NotBeNull();
        result!.CategoryName.Should().Be("New");
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Category>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WithInvalidId_ReturnsNull()
    {
        var id = 999;
        var dto = new UpdateCategoryDto("New");

        _mockRepository.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Category?)null);

        var result = await _service.UpdateAsync(id, dto);

        result.Should().BeNull();
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Category>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WithEmptyName_ThrowsArgumentException()
    {
        var id = 10;
        var existing = new Category { Id = id, CategoryName = "Food", CreatedAt = DateTime.Now };
        var dto = new UpdateCategoryDto("");

        _mockRepository.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(existing);

        var act = async () => await _service.UpdateAsync(id, dto);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WithValidId_ReturnsTrue()
    {
        var id = 20;
        _mockRepository.Setup(r => r.DeleteAsync(id)).ReturnsAsync(true);

        var result = await _service.DeleteAsync(id);

        result.Should().BeTrue();
        _mockRepository.Verify(r => r.DeleteAsync(id), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WithInvalidId_ReturnsFalse()
    {
        var id = 999;
        _mockRepository.Setup(r => r.DeleteAsync(id)).ReturnsAsync(false);

        var result = await _service.DeleteAsync(id);

        result.Should().BeFalse();
    }

    #endregion
}
