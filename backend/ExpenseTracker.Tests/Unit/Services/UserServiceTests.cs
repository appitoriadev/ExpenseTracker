using ExpenseTracker.Application.DTOs;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Application.Services;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace ExpenseTracker.Tests.Unit.Services;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _mockRepository;
    private readonly Mock<ILogger<UserService>> _mockLogger;
    private readonly IUserService _service;

    public UserServiceTests()
    {
        _mockRepository = new Mock<IUserRepository>();
        _mockLogger = new Mock<ILogger<UserService>>();
        _service = new UserService(_mockRepository.Object, _mockLogger.Object);
    }

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsUser()
    {
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Username = "testuser",
            PasswordHash = "hashedpassword",
            FirstName = "Test",
            LastName = "User",
            CreatedAt = DateTime.Now
        };

        _mockRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

        var result = await _service.GetByIdAsync(userId);

        result.Should().NotBeNull();
        result!.Username.Should().Be("testuser");
        result.FirstName.Should().Be("Test");
        _mockRepository.Verify(r => r.GetByIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ReturnsNull()
    {
        var userId = Guid.NewGuid();
        _mockRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync((User?)null);

        var result = await _service.GetByIdAsync(userId);

        result.Should().BeNull();
    }

    #endregion

    #region GetByUsernameAsync Tests

    [Fact]
    public async Task GetByUsernameAsync_WithValidUsername_ReturnsUser()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "johndoe",
            PasswordHash = "hashedpassword",
            FirstName = "John",
            LastName = "Doe",
            CreatedAt = DateTime.Now
        };

        _mockRepository.Setup(r => r.GetByUsernameAsync("johndoe")).ReturnsAsync(user);

        var result = await _service.GetByUsernameAsync("johndoe");

        result.Should().NotBeNull();
        result!.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");
    }

    [Fact]
    public async Task GetByUsernameAsync_WithInvalidUsername_ReturnsNull()
    {
        _mockRepository.Setup(r => r.GetByUsernameAsync("nonexistent")).ReturnsAsync((User?)null);

        var result = await _service.GetByUsernameAsync("nonexistent");

        result.Should().BeNull();
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_WithValidDto_CreatesUser()
    {
        var dto = new CreateUserDto("newuser", "SecurePassword123!", "New", "User");

        var createdUser = new User
        {
            Id = Guid.NewGuid(),
            Username = dto.Username,
            PasswordHash = "hashed",
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            CreatedAt = DateTime.Now
        };

        _mockRepository.Setup(r => r.AddAsync(It.IsAny<User>())).ReturnsAsync(createdUser);

        var result = await _service.CreateAsync(dto);

        result.Should().NotBeNull();
        result.Username.Should().Be("newuser");
        result.FirstName.Should().Be("New");
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WithValidUser_UpdatesUser()
    {
        var userId = Guid.NewGuid();
        var existingUser = new User
        {
            Id = userId,
            Username = "olduser",
            PasswordHash = "hash",
            FirstName = "Old",
            LastName = "User",
            CreatedAt = DateTime.Now
        };

        var updateDto = new UpdateUserDto("olduser", "hash", "Updated", "Name");

        var updatedUser = new User
        {
            Id = userId,
            Username = "olduser",
            PasswordHash = "hash",
            FirstName = "Updated",
            LastName = "Name",
            CreatedAt = DateTime.Now
        };

        _mockRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(existingUser);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<User>())).ReturnsAsync(updatedUser);

        var result = await _service.UpdateAsync(userId, updateDto);

        result.Should().NotBeNull();
        result!.FirstName.Should().Be("Updated");
        result.LastName.Should().Be("Name");
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WithValidId_DeletesUser()
    {
        var userId = Guid.NewGuid();
        _mockRepository.Setup(r => r.DeleteAsync(userId)).ReturnsAsync(true);

        var result = await _service.DeleteAsync(userId);

        result.Should().BeTrue();
        _mockRepository.Verify(r => r.DeleteAsync(userId), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WithInvalidId_ReturnsFalse()
    {
        var userId = Guid.NewGuid();
        _mockRepository.Setup(r => r.DeleteAsync(userId)).ReturnsAsync(false);

        var result = await _service.DeleteAsync(userId);

        result.Should().BeFalse();
    }

    #endregion
}
