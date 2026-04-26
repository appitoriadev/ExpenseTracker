using System.Text;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Application.Services;
using ExpenseTracker.Domain.Interfaces;
using ExpenseTracker.Infrastructure.Data;
using ExpenseTracker.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using dotenv.net;

var builder = WebApplication.CreateBuilder(args);

// Loads .env into the process environment
DotEnv.Load();

// ── Connection Provider ───────────────────────────────────────────────────────
var _connectionString = Environment.GetEnvironmentVariable("CONNECTIONSTRINGS_EXPENSETRACKER")
?? throw new InvalidOperationException("ConnectionString 'ExpenseTracker' not found in configuration.");
builder.Services.AddSingleton(new ConnectionProvider(_connectionString));

// ── Dependency Injection ──────────────────────────────────────────────────────
builder.Services.AddScoped<IExpenseRepository, ExpenseRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserExpenseRepository, UserExpenseRepository>();
builder.Services.AddScoped<IExpenseService, ExpenseService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserExpenseService, UserExpenseService>();
builder.Services.AddHttpContextAccessor();

// ── JWT Authentication ────────────────────────────────────────────────────────
var JWT_KEY = Environment.GetEnvironmentVariable("JWT_KEY")
?? throw new InvalidOperationException("JWT Key is not configured.");

var JWT_ISSUER = Environment.GetEnvironmentVariable("JWT_KEY")
?? throw new InvalidOperationException("JWT Issuer is not configured.");

var JWT_AUDIENCE = Environment.GetEnvironmentVariable("JWT_AUDIENCE")
?? throw new InvalidOperationException("JWT Audience is not configured.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = JWT_ISSUER,
            ValidAudience = JWT_AUDIENCE,
            RequireExpirationTime = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JWT_KEY))
        };
    });

builder.Services.AddAuthorization();

// ── CORS ──────────────────────────────────────────────────────────────────────
var allowedOrigins = builder.Configuration
                            .GetSection("Cors:AllowedOrigins")
                            .Get<string[]>()
                            ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactFrontend", policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// ── Controllers + Swagger ─────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ── Middleware pipeline ───────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("ReactFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();