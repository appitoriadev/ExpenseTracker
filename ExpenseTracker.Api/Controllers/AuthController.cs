using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace ExpenseTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public AuthController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public record LoginRequest(string Username, string Password);
    public record LoginResponse(string Token, DateTime ExpiresAt);

    /// <summary>
    /// Register a user to the application
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Register([FromBody] LoginRequest request)
    {
        return Ok(new { message = "Registration not yet implemented" });
    }


    /// <summary>
    /// Authenticates the single configured user and returns a JWT Bearer token.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var expectedUsername = _configuration["SingleUser:Username"];
        var expectedPassword = _configuration["SingleUser:Password"];

        if (request.Username != expectedUsername ||
            request.Password != expectedPassword)
        {
            return Unauthorized(new { message = "Invalid credentials." });
        }

        var token     = GenerateJwtToken(request.Username);
        var expiresAt = DateTime.UtcNow.AddMinutes(
            int.Parse(_configuration["Jwt:ExpiryMinutes"] ?? "60"));

        return Ok(new LoginResponse(token, expiresAt));
    }

    private string GenerateJwtToken(string username)
    {
        var jwtSection = _configuration.GetSection("Jwt");

        var key   = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSection["Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer:             jwtSection["Issuer"],
            audience:           jwtSection["Audience"],
            claims:             claims,
            expires:            DateTime.UtcNow.AddMinutes(
                                    int.Parse(jwtSection["ExpiryMinutes"] ?? "60")),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
