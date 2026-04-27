using ExpenseTracker.Application.DTOs;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Application.Services;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace ExpenseTracker.Tests.Unit.Services;

public class ExpenseServiceTests
{
    private readonly Mock<IExpenseRepository> _mockRepository;
    private readonly Mock<ICategoryService> _mockCategoryService;
    private readonly Mock<IUserExpenseRepository> _mockUserExpenseRepository;
    private readonly Mock<ILogger<ExpenseService>> _mockLogger;
    private readonly IExpenseService _eService;

    public ExpenseServiceTests()
    {
        _mockRepository = new Mock<IExpenseRepository>();
        _mockCategoryService = new Mock<ICategoryService>();
        _mockUserExpenseRepository = new Mock<IUserExpenseRepository>();
        _mockLogger = new Mock<ILogger<ExpenseService>>();
        _eService = new ExpenseService(_mockRepository.Object, _mockCategoryService.Object, _mockUserExpenseRepository.Object, _mockLogger.Object);
    }

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_WithValidExpenses_ReturnsAllExpenses()
    {
        var expenses = new List<Expense>
        {
            new() { Id = 1, Title = "Lunch", Amount = 15.50m, CategoryName = "Food", Date = DateTime.Now },
            new() { Id = 2, Title = "Gas", Amount = 45.00m, CategoryName = "Transportation", Date = DateTime.Now.AddDays(-1) }
        };

        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(expenses);

        var result = await _eService.GetAllAsync();

        result.Should().HaveCount(2);
        result.First().Title.Should().Be("Lunch");
        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_WithNoExpenses_ReturnsEmptyCollection()
    {
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Expense>());

        var result = await _eService.GetAllAsync();

        result.Should().BeEmpty();
        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsExpense()
    {
        var id = 1;
        var expense = new Expense
        {
            Id = id,
            Title = "Lunch",
            Amount = 15.50m,
            CategoryName = "Food",
            Date = DateTime.Now
        };

        _mockRepository.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(expense);

        var result = await _eService.GetByIdAsync(id);

        result.Should().NotBeNull();
        result!.Title.Should().Be("Lunch");
        result.Amount.Should().Be(15.50m);
        _mockRepository.Verify(r => r.GetByIdAsync(id), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ReturnsNull()
    {
        var id = 999;
        _mockRepository.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Expense?)null);

        var result = await _eService.GetByIdAsync(id);

        result.Should().BeNull();
        _mockRepository.Verify(r => r.GetByIdAsync(id), Times.Once);
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_WithValidDto_CreatesAndReturnsExpense()
    {
        var dto = new CreateExpenseDto("Coffee", 5.50m, "Food", DateTime.Now);
        var categoryId = 10;
        var existingCategory = new CategoryDto(categoryId, "Food", DateTime.Now);

        _mockCategoryService.Setup(c => c.GetByNameAsync("Food")).ReturnsAsync(existingCategory);

        var expenseId = 11;
        var createdExpense = new Expense
        {
            Id = expenseId,
            Title = dto.Title,
            Amount = dto.Amount,
            CategoryName = "Food",
            Date = dto.Date
        };

        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Expense>())).ReturnsAsync(createdExpense);
        _mockUserExpenseRepository.Setup(r => r.AddAsync(It.IsAny<UserExpense>())).ReturnsAsync(new UserExpense { Id = 1, ExpenseId = expenseId, UserId = 1 });

        var result = await _eService.CreateAsync(dto, 1);

        result.Should().NotBeNull();
        result.Title.Should().Be("Coffee");
        result.Amount.Should().Be(5.50m);
        result.Category.Should().Be("Food");
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Expense>()), Times.Once);
        _mockUserExpenseRepository.Verify(r => r.AddAsync(It.IsAny<UserExpense>()), Times.Once);
        _mockCategoryService.Verify(c => c.GetByNameAsync("Food"), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithNonExistentCategory_CreatesCategory()
    {
        var dto = new CreateExpenseDto("Dinner", 45.99m, "Dining", new DateTime(2026, 4, 26));
        var categoryId = 20;

        _mockCategoryService.Setup(c => c.GetByNameAsync("Dining")).ReturnsAsync((CategoryDto?)null);
        _mockCategoryService.Setup(c => c.CreateAsync(It.IsAny<CreateCategoryDto>()))
            .ReturnsAsync(new CategoryDto(categoryId, "Dining", DateTime.Now));

        var expenseId = 21;
        var createdExpense = new Expense
        {
            Id = expenseId,
            Title = dto.Title,
            Amount = dto.Amount,
            CategoryName = "Dining",
            Date = dto.Date
        };

        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Expense>())).ReturnsAsync(createdExpense);
        _mockUserExpenseRepository.Setup(r => r.AddAsync(It.IsAny<UserExpense>())).ReturnsAsync(new UserExpense { Id = 2, ExpenseId = expenseId, UserId = 1 });

        var result = await _eService.CreateAsync(dto, 1);

        result.Should().NotBeNull();
        result.Title.Should().Be("Dinner");
        result.Amount.Should().Be(45.99m);
        _mockCategoryService.Verify(c => c.GetByNameAsync("Dining"), Times.Once);
        _mockCategoryService.Verify(c => c.CreateAsync(It.IsAny<CreateCategoryDto>()), Times.Once);
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Expense>()), Times.Once);
        _mockUserExpenseRepository.Verify(r => r.AddAsync(It.IsAny<UserExpense>()), Times.Once);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WithValidExpense_UpdatesAndReturnsExpense()
    {
        var id = 30;
        var existingExpense = new Expense { Id = id, Title = "Old Title", Amount = 10m, CategoryName = "Old", Date = DateTime.Now };
        var updateDto = new UpdateExpenseDto("New Title", 20m, "New", DateTime.Now);
        var updatedExpense = new Expense { Id = id, Title = "New Title", Amount = 20m, CategoryName = "New", Date = DateTime.Now };

        _mockRepository.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(existingExpense);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Expense>())).ReturnsAsync(updatedExpense);

        var result = await _eService.UpdateAsync(id, updateDto);

        result.Should().NotBeNull();
        result!.Title.Should().Be("New Title");
        result.Amount.Should().Be(20m);
        _mockRepository.Verify(r => r.GetByIdAsync(id), Times.Once);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Expense>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WithInvalidId_ReturnsNull()
    {
        var id = 999;
        var updateDto = new UpdateExpenseDto("Title", 10m, "Cat", DateTime.Now);

        _mockRepository.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Expense?)null);

        var result = await _eService.UpdateAsync(id, updateDto);

        result.Should().BeNull();
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Expense>()), Times.Never);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WithValidId_CallsRepositoryDelete()
    {
        var id = 40;
        _mockRepository.Setup(r => r.DeleteAsync(id)).ReturnsAsync(true);

        var result = await _eService.DeleteAsync(id);

        result.Should().BeTrue();
        _mockRepository.Verify(r => r.DeleteAsync(id), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WithInvalidId_ReturnsFalse()
    {
        var id = 999;
        _mockRepository.Setup(r => r.DeleteAsync(id)).ReturnsAsync(false);

        var result = await _eService.DeleteAsync(id);

        result.Should().BeFalse();
    }

    #endregion
}
