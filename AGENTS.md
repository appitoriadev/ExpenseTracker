# BalastLane API — Agent Specification

Expense Tracker API built with Clean Architecture. This document is for AI agents to understand the codebase structure, constraints, and validation rules.

## Architecture

**Clean Architecture:** Domain ← Application ← (Infrastructure, API)

```
ExpenseTracker.Domain/              → Zero external dependencies
ExpenseTracker.Application/         → Depends only on Domain
ExpenseTracker.Infrastructure/      → Depends only on Domain
ExpenseTracker.Api/                 → Depends on Application + Infrastructure
```

## Tech Stack

- **.NET 10.0** (all projects)
- **PostgreSQL** + **Npgsql** (raw ADO.NET, no EF Core, no ORM)
- **JWT** authentication (env-based config)
- **dotenv.net** for environment loading
- **Swagger** (Swashbuckle.AspNetCore)

## Domain Layer

- **Expense entity**: `Id` (int), `Title`, `Amount`, `Category`, `Date`
- **IExpenseRepository**: `GetAllAsync()`, `GetByIdAsync(id)`, `AddAsync()`, `UpdateAsync()`, `DeleteAsync()`
- No external dependencies

## Application Layer

- **DTOs** (record types): `CreateExpenseDto`, `UpdateExpenseDto`, `ExpenseResponseDto`
- **IExpenseService**: Maps DTOs ↔ Entities, delegates to repository
- **ExpenseService**: Manual mapping, no AutoMapper

## Infrastructure Layer

- **ConnectionProvider**: Manages Npgsql connections from env `CONNECTIONSTRINGS_EXPENSETRACKER`
- **ExpenseRepository**: Raw ADO.NET + Npgsql, parameterized SQL, manual DataReader mapping
- GetAllAsync returns expenses ordered by Date DESC

## API Layer

**Controllers:**
- `AuthController` POST `/api/auth/login` → JWT token
- `ExpensesController` (all endpoints require `[Authorize]`):
  - GET `/api/expenses` → 200 with array
  - GET `/api/expenses/{id:int}` → 200 or 404
  - POST `/api/expenses` → 201 Created
  - PUT `/api/expenses/{id:int}` → 200 or 404
  - DELETE `/api/expenses/{id:int}` → 204 or 404

**Program.cs:**
- Loads env via `DotEnv.Load()` (dotenv.net)
- DI: ConnectionProvider (Singleton), IExpenseRepository, IExpenseService
- JWT auth from env: `JWT_KEY`, `JWT_ISSUER`, `JWT_AUDIENCE`
- CORS from config `Cors:AllowedOrigins`
- Middleware order: CORS → Authentication → Authorization → Controllers
- Swagger enabled in Development

**Configuration:**
- `CONNECTIONSTRINGS_EXPENSETRACKER`: PostgreSQL connection string
- `JWT_KEY`: Signing key (min 32 chars)
- `JWT_ISSUER`, `JWT_AUDIENCE`: Token claims
- `Cors:AllowedOrigins`: Array of allowed origins (e.g., `["http://localhost:5173", "http://localhost:3000"]`)

## Validation Rules

**Expense fields:**
- `Title`: 1–255 characters (required)
- `Amount`: > 0, ≤ 999,999.99 (decimal)
- `Category`: 1–100 characters (required)
- `Date`: DateTime (required)

## Key Constraints

1. **No Entity Framework** — raw ADO.NET only
2. **No AutoMapper** — manual record-based mapping
3. **No validation framework** — inline checks in service
4. **Parameterized queries** — SQL injection prevention
5. **All methods async** — `Task<T>`
6. **Environment-based config** — no hardcoded secrets
7. **NULL safety** — service returns `null` or throws on not found
8. **ProducesResponseType** — all controller methods document HTTP status codes

## Testing Checklist

- [ ] Solution builds without errors
- [ ] All projects reference correct dependencies (no circular refs)
- [ ] Domain has zero external NuGet dependencies
- [ ] JWT auth requires `JWT_KEY`, `JWT_ISSUER`, `JWT_AUDIENCE` env vars
- [ ] Connection string from `CONNECTIONSTRINGS_EXPENSETRACKER`
- [ ] CORS allows React origins from config
- [ ] All Expenses endpoints return correct HTTP status codes
- [ ] GetAllAsync orders by Date DESC
- [ ] Manual SQL uses parameterized queries (@id, @title, etc.)
- [ ] Service methods throw on validation failure
- [ ] Controllers handle exceptions and return appropriate status codes
- [ ] Swagger enabled in Development; no sensitive data in responses

## File Paths

```
ExpenseTracker.Domain/
  Entities/Expense.cs
  Interfaces/IExpenseRepository.cs

ExpenseTracker.Application/
  DTOs/{CreateExpenseDto,UpdateExpenseDto,ExpenseResponseDto}.cs
  Interfaces/IExpenseService.cs
  Services/ExpenseService.cs

ExpenseTracker.Infrastructure/
  Data/ConnectionProvider.cs
  Data/Schema.sql
  Repositories/ExpenseRepository.cs

ExpenseTracker.Api/
  Controllers/{AuthController,ExpensesController}.cs
  Program.cs
```
