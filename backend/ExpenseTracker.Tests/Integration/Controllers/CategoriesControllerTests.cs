using ExpenseTracker.Api.Controllers;
using ExpenseTracker.Application.DTOs;
using ExpenseTracker.Application.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace ExpenseTracker.Tests.Integration.Controllers;

public class CategoriesControllerTests
{
    private readonly Mock<ICategoryService> _mockService;
    private readonly CategoriesController _controller;

    public CategoriesControllerTests()
    {
        _mockService = new Mock<ICategoryService>();
        _controller = new CategoriesController(_mockService.Object);
    }

    #region GetAll Tests

    [Fact]
    public async Task GetAll_WithCategories_ReturnsOkWith200()
    {
        var categories = new List<CategoryDto>
        {
            new(1, "Food",          DateTime.Now),
            new(2, "Transportation", DateTime.Now)
        };

        _mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(categories);

        var result = await _controller.GetAll();

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        var returned = okResult.Value.Should().BeAssignableTo<IEnumerable<CategoryDto>>().Subject;
        returned.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAll_WithNoCategories_ReturnsOkWithEmptyList()
    {
        _mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<CategoryDto>());

        var result = await _controller.GetAll();

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returned = okResult.Value.Should().BeAssignableTo<IEnumerable<CategoryDto>>().Subject;
        returned.Should().BeEmpty();
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_WithValidId_ReturnsOkWithCategory()
    {
        var id = 1;
        var category = new CategoryDto(id, "Food", DateTime.Now);
        _mockService.Setup(s => s.GetByIdAsync(id)).ReturnsAsync(category);

        var result = await _controller.GetById(id);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        var returned = okResult.Value.Should().BeOfType<CategoryDto>().Subject;
        returned.CategoryName.Should().Be("Food");
    }

    [Fact]
    public async Task GetById_WithInvalidId_ReturnsNotFound()
    {
        var id = 999;
        _mockService.Setup(s => s.GetByIdAsync(id)).ReturnsAsync((CategoryDto?)null);

        var result = await _controller.GetById(id);

        result.Should().BeOfType<NotFoundResult>();
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task Create_WithValidDto_ReturnsCreatedAtActionWith201()
    {
        var dto = new CreateCategoryDto("Shopping");
        var created = new CategoryDto(5, "Shopping", DateTime.Now);

        _mockService.Setup(s => s.CreateAsync(dto)).ReturnsAsync(created);

        var result = await _controller.Create(dto);

        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.StatusCode.Should().Be(201);
        var returned = createdResult.Value.Should().BeOfType<CategoryDto>().Subject;
        returned.Id.Should().Be(5);
    }

    [Fact]
    public async Task Create_WithInvalidName_ReturnsBadRequest()
    {
        var dto = new CreateCategoryDto("");

        _mockService.Setup(s => s.CreateAsync(dto))
            .ThrowsAsync(new ArgumentException("Category name is required."));

        var result = await _controller.Create(dto);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_WithValidId_ReturnsOkWithUpdatedCategory()
    {
        var id = 10;
        var dto = new UpdateCategoryDto("Updated");
        var updated = new CategoryDto(id, "Updated", DateTime.Now);

        _mockService.Setup(s => s.UpdateAsync(id, dto)).ReturnsAsync(updated);

        var result = await _controller.Update(id, dto);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task Update_WithInvalidId_ReturnsNotFound()
    {
        var id = 999;
        var dto = new UpdateCategoryDto("Updated");

        _mockService.Setup(s => s.UpdateAsync(id, dto)).ReturnsAsync((CategoryDto?)null);

        var result = await _controller.Update(id, dto);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Update_WithInvalidName_ReturnsBadRequest()
    {
        var id = 10;
        var dto = new UpdateCategoryDto("");

        _mockService.Setup(s => s.UpdateAsync(id, dto))
            .ThrowsAsync(new ArgumentException("Category name is required."));

        var result = await _controller.Update(id, dto);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_WithValidId_ReturnsNoContent()
    {
        var id = 20;
        _mockService.Setup(s => s.DeleteAsync(id)).ReturnsAsync(true);

        var result = await _controller.Delete(id);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_WithInvalidId_ReturnsNotFound()
    {
        var id = 999;
        _mockService.Setup(s => s.DeleteAsync(id)).ReturnsAsync(false);

        var result = await _controller.Delete(id);

        result.Should().BeOfType<NotFoundResult>();
    }

    #endregion
}
