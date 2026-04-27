# BalastLane Backend

REST API for expense tracking with user authentication, built with .NET 10.0 and PostgreSQL.

## Tech Stack

- **.NET 10.0** — ASP.NET Core Web API
- **PostgreSQL 17** — Relational database
- **Npgsql** — Raw ADO.NET data access (no EF Core, no ORM)
- **JWT Bearer** — Authentication with refresh tokens
- **BCrypt.Net-Next** — Password hashing
- **dotenv.net** — Environment variable loading
- **Swashbuckle** — Swagger/OpenAPI documentation
- **xUnit + Moq + FluentAssertions** — Testing framework
- **Testcontainers.PostgreSql** — Integration test containers

## Architecture

Clean Architecture with dependency inversion:

```
ExpenseTracker.Domain         → Zero external NuGet dependencies
ExpenseTracker.Application    → Depends on Domain only
ExpenseTracker.Infrastructure → Depends on Domain only
ExpenseTracker.Api            → Depends on Application + Infrastructure
ExpenseTracker.Tests          → References all projects
```

### Layer Responsibilities

| Layer | Purpose |
|---|---|
| **Domain** | Core entities (Expense, User, Category, UserExpense) and repository interfaces |
| **Application** | DTOs, service interfaces, business logic, JWT token generation |
| **Infrastructure** | PostgreSQL connection management, repository implementations with raw ADO.NET |
| **Api** | Controllers, middleware, JWT configuration, Swagger, CORS |

## Prerequisites

- **.NET 10.0 SDK** or later
- **Docker & Docker Compose** (for containerized setup)
- **PostgreSQL 17** (if running locally without Docker)

## Getting Started

### Local Development

1. Copy environment variables:
   ```bash
   cp ../.env.example .env
   ```

2. Configure the required variables in `.env`:
   - `CONNECTIONSTRINGS_EXPENSETRACKER` — PostgreSQL connection string
   - `JWT_KEY` — Minimum 32 characters
   - `JWT_ISSUER` — Token issuer
   - `JWT_AUDIENCE` — Token audience
   - `JWT_EXPIRYMINUTES` — Token lifetime

3. Restore and build:
   ```bash
   dotnet restore
   dotnet build
   ```

4. Run the API:
   ```bash
   dotnet run --project ExpenseTracker.Api
   ```
   
   - HTTP: `http://localhost:5157`
   - HTTPS: `https://localhost:7156`
   - Swagger: `http://localhost:5157/swagger` (Development only)

### Docker Compose

1. Set environment variables in `.env` or export them:
   ```bash
   export POSTGRES_DB=ExpenseTracker
   export CONNECTIONSTRINGS_EXPENSETRACKER="Host=ExpenseTrackerDb;Port=5432;Database=ExpenseTracker;Username=postgres;Password=password"
   export JWT_KEY="your-256-bit-secret-key-at-least-32-characters-long"
   export JWT_ISSUER="ExpenseTrackerApi"
   export JWT_AUDIENCE="ExpenseTrackerClient"
   export JWT_EXPIRYMINUTES=60
   export ASPNETCORE_ENVIRONMENT=Development
   ```

2. Start services:
   ```bash
   docker-compose up -d
   ```

3. Access the API:
   - HTTP: `http://localhost:5157`
   - HTTPS: `http://localhost:5158`
   - PostgreSQL: `localhost:5432`

## Environment Variables

| Variable | Required | Description |
|---|---|---|
| `CONNECTIONSTRINGS_EXPENSETRACKER` | Yes | PostgreSQL connection string |
| `JWT_KEY` | Yes | JWT signing key (min 32 chars) |
| `JWT_ISSUER` | Yes | Token issuer claim |
| `JWT_AUDIENCE` | Yes | Token audience claim |
| `JWT_EXPIRYMINUTES` | Yes | Token lifetime in minutes |
| `ASPNETCORE_ENVIRONMENT` | No | Runtime environment (default: Development) |
| `POSTGRES_USER` | Docker only | Database user |
| `POSTGRES_PASSWORD` | Docker only | Database password |
| `POSTGRES_DB` | Docker only | Database name |

## API Reference

### Authentication Endpoints

| Method | Route | Auth Required | Response |
|---|---|---|---|
| POST | `/api/auth/register` | No | 201 AuthResponse / 400 |
| POST | `/api/auth/login` | No | 200 AuthResponse / 401 |
| POST | `/api/auth/refresh` | No | 200 AuthResponse / 401 |

**AuthResponse** includes:
- `UserId` — User ID
- `Username` — Username
- `Token` — JWT access token
- `RefreshToken` — Refresh token (7-day expiry)
- `ExpiresAt` — JWT expiry timestamp

### Expenses Endpoints `[Authorize]`

| Method | Route | Response |
|---|---|---|
| GET | `/api/expenses` | 200 array (ordered by date DESC) |
| GET | `/api/expenses/{id}` | 200 / 404 |
| POST | `/api/expenses` | 201 / 400 |
| PUT | `/api/expenses/{id}` | 200 / 404 |
| DELETE | `/api/expenses/{id}` | 204 / 404 |

### Categories Endpoints `[Authorize]`

| Method | Route | Response |
|---|---|---|
| GET | `/api/categories` | 200 |
| GET | `/api/categories/{id}` | 200 / 404 |
| POST | `/api/categories` | 201 / 400 |
| PUT | `/api/categories/{id}` | 200 / 404 / 400 |
| DELETE | `/api/categories/{id}` | 204 / 404 |

### Users Endpoints `[Authorize]`

| Method | Route | Response |
|---|---|---|
| GET | `/api/users` | 200 |
| GET | `/api/users/{id}` | 200 / 404 |
| GET | `/api/users/by-username/{username}` | 200 / 404 |
| POST | `/api/users` | 201 / 400 |
| PUT | `/api/users/{id}` | 200 / 404 / 400 |
| DELETE | `/api/users/{id}` | 204 / 404 |

### User Expenses Endpoints `[Authorize]`

| Method | Route | Response |
|---|---|---|
| GET | `/api/userexpenses/by-user/{userId}` | 200 |
| GET | `/api/userexpenses/{id}` | 200 / 404 |
| POST | `/api/userexpenses` | 201 |
| DELETE | `/api/userexpenses/{id}` | 204 / 404 |

## Authentication Flow

1. **Register** — POST `/api/auth/register` with username, email, password, first name, last name
2. **Login** — POST `/api/auth/login` with username and password → returns JWT + refresh token
3. **Access Protected Endpoints** — Include JWT in `Authorization: Bearer {token}` header
4. **Refresh Token** — POST `/api/auth/refresh` with refresh token body → returns new JWT + refresh token

## Error Handling

Global exception middleware returns structured JSON:

```json
{
  "Message": "Error description",
  "TransactionId": "trace-id",
  "Timestamp": "2024-01-01T00:00:00Z",
  "Errors": {
    "Field": ["Error message"]
  }
}
```

| Exception Type | HTTP Status |
|---|---|
| `ValidationException` | 400 (with field errors) |
| `NotFoundException` | 404 |
| `UnauthorizedAccessException` | 401 |
| `ArgumentException` | 400 |
| Other | 500 |

## Database Schema

Located in `Data/Schema.sql` — runs automatically in Docker via `docker-entrypoint-initdb.d`.

### Tables

| Table | Description |
|---|---|
| `dbo.categories` | Expense categories with unique names |
| `dbo.expenses` | Expense records with foreign key to categories |
| `dbo.users` | User accounts with password hash and refresh tokens |
| `dbo.user_expenses` | Junction table linking users to expenses |

### Indexes

- `idx_expenses_date` — on `expense_date DESC` for ordered queries
- `idx_expenses_category` — on `category_id`
- `idx_users_username` — on `username`
- `idx_userexpenses_user` — on `user_id`
- `idx_userexpenses_expense` — on `expense_id`

## CORS Configuration

Allowed origins configured in `ExpenseTracker.Api/appsettings.json`:

```json
"Cors": {
  "AllowedOrigins": [
    "http://localhost:5173",
    "http://localhost:3000"
  ]
}
```

## Testing

Run all tests:
```bash
dotnet test
```

Run with coverage:
```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Test Structure

- `Unit/Entities/` — Entity validation tests
- `Unit/Services/` — Service logic tests (ExpenseService, UserService)
- `Integration/Controllers/` — Controller endpoint tests (AuthController, ExpensesController)
- `Integration/Repositories/` — Repository integration tests with Testcontainers

### Test Stack

- **xUnit** — Test framework
- **Moq** — Mocking library
- **FluentAssertions** — Assertion library
- **Testcontainers.PostreSql** — PostgreSQL container for integration tests
- **coverlet.collector** — Code coverage

## Project Structure

```
backend/
├── Data/
│   └── Schema.sql                 # Database schema
├── ExpenseTracker.Api/
│   ├── Controllers/
│   │   ├── AuthController.cs
│   │   ├── CategoriesController.cs
│   │   ├── ExpensesController.cs
│   │   ├── UsersController.cs
│   │   └── UserExpensesController.cs
│   ├── Middleware/
│   │   └── ExceptionHandlingMiddleware.cs
│   ├── Properties/
│   │   └── launchSettings.json
│   ├── Program.cs                 # App configuration & DI
│   ├── appsettings.json           # CORS & logging config
│   └── ExpenseTracker.Api.csproj
├── ExpenseTracker.Application/
│   ├── DTOs/                      # Request/Response DTOs
│   ├── Interfaces/                # Service interfaces
│   ├── Services/                  # Business logic
│   └── ExpenseTracker.Application.csproj
├── ExpenseTracker.Domain/
│   ├── Entities/                  # Domain entities
│   ├── Interfaces/                # Repository interfaces
│   └── ExpenseTracker.Domain.csproj
├── ExpenseTracker.Infrastructure/
│   ├── Data/
│   │   └── ConnectionProvider.cs  # Npgsql connection management
│   └── Repositories/              # ADO.NET repository implementations
├── ExpenseTracker.Tests/
│   ├── Fixtures/                  # Test fixtures
│   ├── Helpers/                   # Test helpers (JWT generation)
│   ├── Integration/               # Integration tests
│   └── Unit/                      # Unit tests
├── Dockerfile                     # Multi-stage Docker build
├── docker-compose.yml             # PostgreSQL + API containers
├── ExpenseTracker.sln             # Solution file
└── README.md
```

## Key Design Decisions

- **No Entity Framework** — Raw ADO.NET with parameterized SQL for performance and control
- **No AutoMapper** — Manual record-based mapping for explicit data flow
- **No validation framework** — Inline validation in services for simplicity
- **Refresh Tokens** — Stored in database with 7-day expiry for token rotation
- **Clean Architecture** — Dependency inversion for testability and maintainability
