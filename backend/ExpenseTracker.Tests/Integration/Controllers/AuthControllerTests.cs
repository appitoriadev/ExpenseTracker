using ExpenseTracker.Application.DTOs;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Api.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace ExpenseTracker.Tests.Integration.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _mockAuthService;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _mockAuthService = new Mock<IAuthService>();
        _controller = new AuthController(_mockAuthService.Object);
    }

    #region Register Tests

    [Fact]
    public async Task Register_WithValidRequest_ReturnsCreated()
    {
        var request = new RegisterRequest("testuser", "test@example.com", "password123", "Test", "User");
        var authResponse = new AuthResponse(
            UserId: 1,
            Username: "testuser",
            Token: "jwt-token-here",
            RefreshToken: "refresh-token-here",
            ExpiresAt: DateTime.UtcNow.AddMinutes(60)
        );

        _mockAuthService.Setup(s => s.RegisterAsync(request))
            .ReturnsAsync(authResponse);

        var result = await _controller.Register(request);

        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task Register_WithDuplicateUsername_ReturnsBadRequest()
    {
        var request = new RegisterRequest("existinguser", "new@example.com", "password123", "Test", "User");

        _mockAuthService.Setup(s => s.RegisterAsync(request))
            .ThrowsAsync(new InvalidOperationException("Username is already taken."));

        var result = await _controller.Register(request);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsBadRequest()
    {
        var request = new RegisterRequest("newuser", "existing@example.com", "password123", "Test", "User");

        _mockAuthService.Setup(s => s.RegisterAsync(request))
            .ThrowsAsync(new InvalidOperationException("Email is already registered."));

        var result = await _controller.Register(request);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region Login Tests

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkWithToken()
    {
        var request = new LoginRequest("testuser", "password123");
        var authResponse = new AuthResponse(
            UserId: 2,
            Username: "testuser",
            Token: "jwt-token-here",
            RefreshToken: "refresh-token-here",
            ExpiresAt: DateTime.UtcNow.AddMinutes(60)
        );

        _mockAuthService.Setup(s => s.LoginAsync(request))
            .ReturnsAsync(authResponse);

        var result = await _controller.Login(request);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var response = okResult.Value.Should().BeOfType<AuthResponse>().Subject;
        response.Token.Should().NotBeNullOrEmpty();
        response.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task Login_WithInvalidUsername_ReturnsUnauthorized()
    {
        var request = new LoginRequest("nonexistent", "password123");

        _mockAuthService.Setup(s => s.LoginAsync(request))
            .ThrowsAsync(new InvalidOperationException("Invalid credentials."));

        var result = await _controller.Login(request);

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsUnauthorized()
    {
        var request = new LoginRequest("testuser", "wrongpassword");

        _mockAuthService.Setup(s => s.LoginAsync(request))
            .ThrowsAsync(new InvalidOperationException("Invalid credentials."));

        var result = await _controller.Login(request);

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    #endregion

    #region Refresh Token Tests

    [Fact]
    public async Task Refresh_WithValidRefreshToken_ReturnsNewToken()
    {
        var refreshToken = "valid-refresh-token";
        var authResponse = new AuthResponse(
            UserId: 3,
            Username: "testuser",
            Token: "new-jwt-token",
            RefreshToken: "new-refresh-token",
            ExpiresAt: DateTime.UtcNow.AddMinutes(60)
        );

        _mockAuthService.Setup(s => s.RefreshTokenAsync(refreshToken))
            .ReturnsAsync(authResponse);

        var result = await _controller.Refresh(refreshToken);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var response = okResult.Value.Should().BeOfType<AuthResponse>().Subject;
        response.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Refresh_WithExpiredRefreshToken_ReturnsUnauthorized()
    {
        var refreshToken = "expired-refresh-token";

        _mockAuthService.Setup(s => s.RefreshTokenAsync(refreshToken))
            .ThrowsAsync(new InvalidOperationException("Refresh token has expired."));

        var result = await _controller.Refresh(refreshToken);

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Refresh_WithInvalidRefreshToken_ReturnsUnauthorized()
    {
        var refreshToken = "invalid-refresh-token";

        _mockAuthService.Setup(s => s.RefreshTokenAsync(refreshToken))
            .ThrowsAsync(new InvalidOperationException("Invalid refresh token."));

        var result = await _controller.Refresh(refreshToken);

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    #endregion
}
