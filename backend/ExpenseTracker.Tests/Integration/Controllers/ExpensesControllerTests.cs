using ExpenseTracker.Api.Controllers;
using ExpenseTracker.Application.DTOs;
using ExpenseTracker.Application.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace ExpenseTracker.Tests.Integration.Controllers;

public class ExpensesControllerTests
{
    private readonly Mock<IExpenseService> _mockService;
    private readonly ExpensesController _controller;

    public ExpensesControllerTests()
    {
        _mockService = new Mock<IExpenseService>();
        _controller = new ExpensesController(_mockService.Object);
    }

    #region GetAll Tests

    [Fact]
    public async Task GetAll_WithValidExpenses_ReturnsOkWith200()
    {
        var expenses = new List<ExpenseResponseDto>
        {
            new(Guid.NewGuid(), "Lunch", 15.50m, "Food", DateTime.Now),
            new(Guid.NewGuid(), "Gas", 45.00m, "Transportation", DateTime.Now)
        };

        _mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(expenses);

        var result = await _controller.GetAll();

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        var returnedExpenses = okResult.Value.Should().BeAssignableTo<IEnumerable<ExpenseResponseDto>>().Subject;
        returnedExpenses.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAll_WithNoExpenses_ReturnsOkWithEmptyList()
    {
        _mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<ExpenseResponseDto>());

        var result = await _controller.GetAll();

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedExpenses = okResult.Value.Should().BeAssignableTo<IEnumerable<ExpenseResponseDto>>().Subject;
        returnedExpenses.Should().BeEmpty();
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_WithValidId_ReturnsOkWithExpense()
    {
        var id = Guid.NewGuid();
        var expense = new ExpenseResponseDto(id, "Coffee", 5.50m, "Food", DateTime.Now);
        _mockService.Setup(s => s.GetByIdAsync(id)).ReturnsAsync(expense);

        var result = await _controller.GetById(id);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        var returnedExpense = okResult.Value.Should().BeOfType<ExpenseResponseDto>().Subject;
        returnedExpense.Title.Should().Be("Coffee");
    }

    [Fact]
    public async Task GetById_WithInvalidId_ReturnsNotFound()
    {
        var id = Guid.NewGuid();
        _mockService.Setup(s => s.GetByIdAsync(id)).ReturnsAsync((ExpenseResponseDto?)null);

        var result = await _controller.GetById(id);

        result.Should().BeOfType<NotFoundResult>();
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task Create_WithValidDto_ReturnsCreatedAtActionWith201()
    {
        var createDto = new CreateExpenseDto("New Expense", 100m, "Test", DateTime.Now);
        var createdId = Guid.NewGuid();
        var createdDto = new ExpenseResponseDto(createdId, "New Expense", 100m, "Test", DateTime.Now);

        _mockService.Setup(s => s.CreateAsync(createDto)).ReturnsAsync(createdDto);

        var result = await _controller.Create(createDto);

        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.StatusCode.Should().Be(201);
        createdResult.ActionName.Should().Be(nameof(ExpensesController.GetById));
        var returnedExpense = createdResult.Value.Should().BeOfType<ExpenseResponseDto>().Subject;
        returnedExpense.Id.Should().Be(createdId);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_WithValidId_ReturnsOkWithUpdatedExpense()
    {
        var id = Guid.NewGuid();
        var updateDto = new UpdateExpenseDto("Updated", 200m, "Updated", DateTime.Now);
        var updatedDto = new ExpenseResponseDto(id, "Updated", 200m, "Updated", DateTime.Now);

        _mockService.Setup(s => s.UpdateAsync(id, updateDto)).ReturnsAsync(updatedDto);

        var result = await _controller.Update(id, updateDto);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task Update_WithInvalidId_ReturnsNotFound()
    {
        var id = Guid.NewGuid();
        var updateDto = new UpdateExpenseDto("Updated", 200m, "Updated", DateTime.Now);

        _mockService.Setup(s => s.UpdateAsync(id, updateDto)).ReturnsAsync((ExpenseResponseDto?)null);

        var result = await _controller.Update(id, updateDto);

        result.Should().BeOfType<NotFoundResult>();
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_WithValidId_ReturnsNoContent()
    {
        var id = Guid.NewGuid();
        _mockService.Setup(s => s.DeleteAsync(id)).ReturnsAsync(true);

        var result = await _controller.Delete(id);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_WithInvalidId_ReturnsNotFound()
    {
        var id = Guid.NewGuid();
        _mockService.Setup(s => s.DeleteAsync(id)).ReturnsAsync(false);

        var result = await _controller.Delete(id);

        result.Should().BeOfType<NotFoundResult>();
    }

    #endregion
}
