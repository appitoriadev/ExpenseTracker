using ExpenseTracker.Api.Controllers;
using ExpenseTracker.Application.DTOs;
using ExpenseTracker.Application.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace ExpenseTracker.Tests.Integration.Controllers;

public class UsersControllerTests
{
    private readonly Mock<IUserService> _mockService;
    private readonly UsersController _controller;

    public UsersControllerTests()
    {
        _mockService = new Mock<IUserService>();
        _controller = new UsersController(_mockService.Object);
    }

    #region GetAll Tests

    [Fact]
    public async Task GetAll_WithUsers_ReturnsOkWith200()
    {
        var users = new List<UserDto>
        {
            new(1, "alice", "hash1", "Alice", "Smith",  DateTime.Now),
            new(2, "bob",   "hash2", "Bob",   "Jones",  DateTime.Now)
        };

        _mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(users);

        var result = await _controller.GetAll();

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        var returned = okResult.Value.Should().BeAssignableTo<IEnumerable<UserDto>>().Subject;
        returned.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAll_WithNoUsers_ReturnsOkWithEmptyList()
    {
        _mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<UserDto>());

        var result = await _controller.GetAll();

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returned = okResult.Value.Should().BeAssignableTo<IEnumerable<UserDto>>().Subject;
        returned.Should().BeEmpty();
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_WithValidId_ReturnsOkWithUser()
    {
        var id = 1;
        var user = new UserDto(id, "alice", "hash", "Alice", "Smith", DateTime.Now);
        _mockService.Setup(s => s.GetByIdAsync(id)).ReturnsAsync(user);

        var result = await _controller.GetById(id);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        var returned = okResult.Value.Should().BeOfType<UserDto>().Subject;
        returned.Username.Should().Be("alice");
    }

    [Fact]
    public async Task GetById_WithInvalidId_ReturnsNotFound()
    {
        var id = 999;
        _mockService.Setup(s => s.GetByIdAsync(id)).ReturnsAsync((UserDto?)null);

        var result = await _controller.GetById(id);

        result.Should().BeOfType<NotFoundResult>();
    }

    #endregion

    #region GetByUsername Tests

    [Fact]
    public async Task GetByUsername_WithValidUsername_ReturnsOkWithUser()
    {
        var user = new UserDto(2, "bob", "hash", "Bob", "Jones", DateTime.Now);
        _mockService.Setup(s => s.GetByUsernameAsync("bob")).ReturnsAsync(user);

        var result = await _controller.GetByUsername("bob");

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        var returned = okResult.Value.Should().BeOfType<UserDto>().Subject;
        returned.Username.Should().Be("bob");
    }

    [Fact]
    public async Task GetByUsername_WithInvalidUsername_ReturnsNotFound()
    {
        _mockService.Setup(s => s.GetByUsernameAsync("ghost")).ReturnsAsync((UserDto?)null);

        var result = await _controller.GetByUsername("ghost");

        result.Should().BeOfType<NotFoundResult>();
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task Create_WithValidDto_ReturnsCreatedAtActionWith201()
    {
        var dto = new CreateUserDto("newuser", "hashedpw", "New", "User");
        var created = new UserDto(3, "newuser", "hashedpw", "New", "User", DateTime.Now);

        _mockService.Setup(s => s.CreateAsync(dto)).ReturnsAsync(created);

        var result = await _controller.Create(dto);

        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.StatusCode.Should().Be(201);
        var returned = createdResult.Value.Should().BeOfType<UserDto>().Subject;
        returned.Id.Should().Be(3);
        returned.Username.Should().Be("newuser");
    }

    [Fact]
    public async Task Create_WithInvalidDto_ReturnsBadRequest()
    {
        var dto = new CreateUserDto("", "", "", "");

        _mockService.Setup(s => s.CreateAsync(dto))
            .ThrowsAsync(new ArgumentException("Username is required."));

        var result = await _controller.Create(dto);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_WithValidId_ReturnsOkWithUpdatedUser()
    {
        var id = 4;
        var dto = new UpdateUserDto("updateduser", "newhash", "Updated", "Name");
        var updated = new UserDto(id, "updateduser", "newhash", "Updated", "Name", DateTime.Now);

        _mockService.Setup(s => s.UpdateAsync(id, dto)).ReturnsAsync(updated);

        var result = await _controller.Update(id, dto);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        var returned = okResult.Value.Should().BeOfType<UserDto>().Subject;
        returned.FirstName.Should().Be("Updated");
    }

    [Fact]
    public async Task Update_WithInvalidId_ReturnsNotFound()
    {
        var id = 999;
        var dto = new UpdateUserDto("user", "hash", "First", "Last");

        _mockService.Setup(s => s.UpdateAsync(id, dto)).ReturnsAsync((UserDto?)null);

        var result = await _controller.Update(id, dto);

        result.Should().BeOfType<NotFoundResult>();
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_WithValidId_ReturnsNoContent()
    {
        var id = 5;
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
