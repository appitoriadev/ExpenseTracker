namespace ExpenseTracker.Application.DTOs;

public record AuthResponse(
    int UserId,
    string Username,
    string Token,
    string RefreshToken,
    DateTime ExpiresAt
);
