# Expense Tracker — Clean Architecture .NET 10 Web API

## Context

Scaffold a greenfield C# .NET 10 Web API for an Expense Tracker application structured
in four Clean Architecture layers. Consumed by a React frontend. Part of a tech interview.

---

## Solution Structure

```batch
ExpenseTracker/
├── ExpenseTracker.sln
└── 
    ├── ExpenseTracker.Domain/          ← zero external deps
    │   ├── Entities/
    │   │   ├── Expense.cs
    │   │   ├── Category.cs
    │   │   ├── User.cs
    │   │   └── UserExpense.cs
    │   └── Interfaces/
    │       ├── IExpenseRepository.cs
    │       ├── ICategoryRepository.cs
    │       ├── IUserRepository.cs
    │       └── IUserExpenseRepository.cs
    ├── ExpenseTracker.Application/     ← depends on Domain only
    │   ├── DTOs/
    │   │   ├── {Create,Update,Response}ExpenseDto.cs
    │   │   ├── {Create,Update}CategoryDto.cs
    │   │   ├── {Create,Update}UserDto.cs
    │   │   ├── AuthResponse.cs
    │   │   ├── LoginRequest.cs
    │   │   └── UserExpenseDto.cs
    │   ├── Interfaces/
    │   │   ├── IExpenseService.cs
    │   │   ├── ICategoryService.cs
    │   │   ├── IUserService.cs
    │   │   ├── IUserExpenseService.cs
    │   │   └── IAuthService.cs
    │   └── Services/
    │       ├── ExpenseService.cs
    │       ├── CategoryService.cs
    │       ├── UserService.cs
    │       ├── UserExpenseService.cs
    │       └── AuthService.cs
    ├── ExpenseTracker.Infrastructure/  ← depends on Domain only
    │   ├── Data/
    │   │   ├── ConnectionProvider.cs
    │   │   └── Schema.sql
    │   └── Repositories/
    │       ├── ExpenseRepository.cs
    │       ├── CategoryRepository.cs
    │       ├── UserRepository.cs
    │       └── UserExpenseRepository.cs
    ├── ExpenseTracker.Api/             ← depends on Application + Infrastructure
    │   ├── Controllers/
    │   │   ├── AuthController.cs
    │   │   ├── ExpensesController.cs
    │   │   ├── CategoriesController.cs
    │   │   ├── UsersController.cs
    │   │   └── UserExpensesController.cs
    │   ├── Middleware/
    │   │   └── ExceptionHandlingMiddleware.cs
    │   ├── Program.cs
    │   ├── appsettings.json
    │   └── appsettings.Development.json
    └── ExpenseTracker.Tests/
        ├── Unit/
        │   ├── Services/
        │   │   ├── ExpenseServiceTests.cs       (95+ test cases)
        │   │   ├── UserServiceTests.cs          (60+ test cases)
        │   │   └── CategoryServiceTests.cs
        │   └── Entities/
        │       └── ExpenseTests.cs              (5+ test cases)
        ├── Integration/
        │   ├── Repositories/
        │   │   ├── ExpenseRepositoryTests.cs    (10+ test cases)
        │   │   └── UserRepositoryTests.cs
        │   └── Controllers/
        │       ├── ExpensesControllerTests.cs   (10+ test cases)
        │       ├── AuthControllerTests.cs       (8+ test cases)
        │       ├── CategoriesControllerTests.cs
        │       └── UsersControllerTests.cs
        ├── Fixtures/
        │   ├── DatabaseFixture.cs               (PostgreSQL test container)
        │   └── DatabaseCollection.cs            (xUnit collection definition)
        ├── Helpers/
        │   └── JwtTokenHelper.cs                (JWT token generation utilities)
        ├── README.md                            (Detailed test documentation)
        └── ExpenseTracker.Tests.csproj

```

**Dependency rule:** Domain ← Application ← (Infrastructure, API). Infrastructure references
Domain only; API references Application + Infrastructure for DI wiring.

---

## Phase 1 — CLI Scaffold

```bash
dotnet new sln -n ExpenseTracker

dotnet new classlib -n ExpenseTracker.Domain -f net10.0 -o ExpenseTracker.Domain
dotnet new classlib -n ExpenseTracker.Application -f net10.0 -o ExpenseTracker.Application
dotnet new classlib -n ExpenseTracker.Infrastructure -f net10.0 -o ExpenseTracker.Infrastructure
dotnet new webapi -n ExpenseTracker.Api -f net10.0 -o ExpenseTracker.Api
dotnet new xunit -n ExpenseTracker.Tests -f net10.0 -o ExpenseTracker.Tests

dotnet sln ExpenseTracker.sln add \
  ExpenseTracker.Domain/ExpenseTracker.Domain.csproj \
  ExpenseTracker.Application/ExpenseTracker.Application.csproj \
  ExpenseTracker.Infrastructure/ExpenseTracker.Infrastructure.csproj \
  ExpenseTracker.Api/ExpenseTracker.Api.csproj \
  ExpenseTracker.Tests/ExpenseTracker.Tests.csproj

# Project references
dotnet add ExpenseTracker.Application/ExpenseTracker.Application.csproj \
    reference ExpenseTracker.Domain/ExpenseTracker.Domain.csproj

dotnet add ExpenseTracker.Infrastructure/ExpenseTracker.Infrastructure.csproj \
    reference ExpenseTracker.Domain/ExpenseTracker.Domain.csproj

dotnet add ExpenseTracker.Api/ExpenseTracker.Api.csproj \
    reference ExpenseTracker.Application/ExpenseTracker.Application.csproj
dotnet add ExpenseTracker.Api/ExpenseTracker.Api.csproj \
    reference ExpenseTracker.Infrastructure/ExpenseTracker.Infrastructure.csproj

dotnet add ExpenseTracker.Tests/ExpenseTracker.Tests.csproj \
  reference ExpenseTracker.Domain/ExpenseTracker.Domain.csproj \
  reference ExpenseTracker.Application/ExpenseTracker.Application.csproj \
  reference ExpenseTracker.Infrastructure/ExpenseTracker.Infrastructure.csproj \
  reference ExpenseTracker.Api/ExpenseTracker.Api.csproj

# NuGet packages
dotnet add ExpenseTracker.Infrastructure/ExpenseTracker.Infrastructure.csproj \
    package Npgsql

dotnet add ExpenseTracker.Api/ExpenseTracker.Api.csproj \
    package Microsoft.AspNetCore.Authentication.JwtBearer

dotnet add ExpenseTracker.Api/ExpenseTracker.Api.csproj \
    package Swashbuckle.AspNetCore
    
dotnet add ExpenseTracker.Tests/ExpenseTracker.Tests.csproj \
  package Moq \
  package FluentAssertions \
  package Testcontainers.PostgreSql

```

---

## Phase 2 — Source Files

### Domain

**`Entities/Expense.cs`** — Plain POCO: Id (int), Title, Amount, CategoryName, Date  
**`Entities/Category.cs`** — Plain POCO: Id (int), CategoryName  
**`Entities/User.cs`** — Plain POCO: Id (int), Username, PasswordHash, FirstName, LastName, Email, RefreshToken, RefreshTokenExpiry  
**`Entities/UserExpense.cs`** — Junction entity: Id (int), ExpensesId (int), UserId (int)  
**`Interfaces/IExpenseRepository.cs`** — GetAll, GetById, Add, Update, Delete, GetByNameAsync (all async)  
**`Interfaces/ICategoryRepository.cs`** — GetAll, GetById, Add, Update, Delete, GetByNameAsync (all async)  
**`Interfaces/IUserRepository.cs`** — GetAll, GetById, Add, Update, Delete, GetByUsername (all async)  
**`Interfaces/IUserExpenseRepository.cs`** — GetAll, GetById, Add, Delete (all async)

### Application

DTOs as C# `record` types (immutable value carriers, no AutoMapper dependency).  
**`Services/ExpenseService.cs`** — maps entities↔DTOs, delegates to `IExpenseRepository`  
**`Services/CategoryService.cs`** — manages category CRUD operations, delegates to `ICategoryRepository`  
**`Services/UserService.cs`** — manages user CRUD operations, delegates to `IUserRepository`  
**`Services/UserExpenseService.cs`** — manages user-expense relationships, delegates to `IUserExpenseRepository`  
**`Services/AuthService.cs`** — handles user authentication, JWT token generation with refresh tokens, BCrypt password hashing

### Infrastructure

**`Data/ConnectionProvider.cs`** — Singleton service providing NpgsqlConnection with connection string from appsettings  
**`Repositories/ExpenseRepository.cs`** — Raw ADO.NET + Npgsql implementation with manual SQL queries and DataReader mapping  
**`Repositories/CategoryRepository.cs`** — Raw ADO.NET + Npgsql implementation for category CRUD  
**`Repositories/UserRepository.cs`** — Raw ADO.NET + Npgsql implementation for user CRUD and authentication lookups  
**`Repositories/UserExpenseRepository.cs`** — Raw ADO.NET + Npgsql implementation for user-expense relationships  
**`Data/Schema.sql`** — PostgreSQL schema with `dbo` schema, tables: categories, expenses, users, user_expenses with foreign key constraints

### API

**`Controllers/AuthController.cs`** — `POST /api/auth/login` authenticates users, returns JWT + refresh token, handles token refresh  
**`Controllers/ExpensesController.cs`** — 5 verbs (GET all, GET/:id, POST, PUT/:id, DELETE/:id), all decorated `[Authorize]`  
**`Controllers/CategoriesController.cs`** — Category CRUD endpoints (GET all, GET/:id, POST, PUT/:id, DELETE/:id), all decorated `[Authorize]`  
**`Controllers/UsersController.cs`** — User management endpoints (GET all, GET/:id, POST, PUT/:id, DELETE/:id), all decorated `[Authorize]`  
**`Controllers/UserExpensesController.cs`** — User-expense relationship endpoints, all decorated `[Authorize]`  
**`Middleware/ExceptionHandlingMiddleware.cs`** — Centralized error handling and logging for all exceptions  
**`Program.cs`** — registers ConnectionProvider (DI), JWT Bearer, CORS (React origins), Swagger with Bearer button, ExceptionHandlingMiddleware  
**`appsettings.json`** — JWT key/issuer/audience/expiry, CORS origins (`localhost:5173`, `localhost:3000`), PostgreSQL connection string

Key middleware order in `Program.cs`:

```bash
UseCors → UseAuthentication → UseAuthorization → MapControllers
```

CORS before auth so browser pre-flight OPTIONS requests succeed before JWT inspection.

---

## Phase 3 — Database Schema

Create tables via SQL script in `Infrastructure/Data/Schema.sql`:

```sql
CREATE SCHEMA IF NOT EXISTS dbo;

CREATE TABLE IF NOT EXISTS dbo.categories (
  id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  category_name VARCHAR(255) NOT NULL UNIQUE,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS dbo.expenses (
  id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  title VARCHAR(255) NOT NULL,
  amount NUMERIC(18, 2) NOT NULL CHECK (amount > 0),
  category_name VARCHAR(255) NOT NULL,
  date TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  CONSTRAINT fk_categories FOREIGN KEY (category_name) REFERENCES dbo.categories (category_name)
);

CREATE TABLE IF NOT EXISTS dbo.users (
  id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  username VARCHAR(255) UNIQUE NOT NULL,
  password_hash VARCHAR(255) NOT NULL,
  firstname VARCHAR(255) NOT NULL,
  lastname VARCHAR(255) NOT NULL,
  email VARCHAR(255) UNIQUE,
  refresh_token VARCHAR(512),
  refresh_token_expiry TIMESTAMP,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS dbo.user_expenses (
  id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  expenses_id INT NOT NULL,
  user_id INT NOT NULL,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  CONSTRAINT fk_user FOREIGN KEY (user_id) REFERENCES dbo.users (id),
  CONSTRAINT fk_expenses FOREIGN KEY (expenses_id) REFERENCES dbo.expenses (id)
);

CREATE INDEX IF NOT EXISTS idx_expenses_date ON dbo.expenses (date DESC);
CREATE INDEX IF NOT EXISTS idx_users_username ON dbo.users (username);
CREATE INDEX IF NOT EXISTS idx_userexpenses_ids ON dbo.user_expenses (user_id);
CREATE INDEX IF NOT EXISTS idx_expenses_ids ON dbo.user_expenses (expenses_id);
```

Execute manually on database before first run.

---

## Verification

**Pre-run:** Ensure PostgreSQL is running and the `expenses` table exists (run schema SQL script)

```bash
# Build
dotnet build ExpenseTracker.sln   # expect: 0 errors

# Run
dotnet run --project ExpenseTracker.Api/ExpenseTracker.Api.csproj
# Swagger at https://localhost:<port>/swagger

# Quick smoke test
TOKEN=$(curl -X 'POST' \
  'http://localhost:5157/api/Auth/login' \
  -H 'accept: text/plain' \
  -H 'Content-Type: application/json' \
  -d '{
  "username": "admin",
  "password": "P@ssw0rd!"
}' | jq -r '.token')


curl -s https://localhost:<port>/api/expenses -H "Authorization: Bearer $TOKEN"  # 200
curl -s https://localhost:<port>/api/expenses                                      # 401
```

---

## Phase 4 — Test Suite (TDD)

### Testing Strategy by Layer

#### Unit Tests — Services (Mocked Repositories)

**File:** `Unit/Services/ExpenseServiceTests.cs`, `UserServiceTests.cs`

- Mock `IExpenseRepository`, `IUserRepository` using Moq
- Test business logic **in isolation** from database
- Verify DTO ↔ Entity mapping
- Test edge cases: null returns, empty collections, invalid IDs

**Example:**

```csharp
[Fact]
public async Task CreateAsync_WithValidDto_CreatesAndReturnsExpense()
{
    var mockRepo = new Mock<IExpenseRepository>();
    var service = new ExpenseService(mockRepo.Object);
    
    var dto = new CreateExpenseDto("Coffee", 5.50m, "Food", DateTime.Now);
    var result = await service.CreateAsync(dto);
    
    result.Should().NotBeNull();
    mockRepo.Verify(r => r.AddAsync(It.IsAny<Expense>()), Times.Once);
}
```

**Coverage:** 80%+ of service methods, happy path + edge cases.

---

#### Unit Tests — Entities (Domain Models)

**File:** `Unit/Entities/ExpenseTests.cs`

- Test POCO property assignment and default values
- Verify entity mutability (required for ORMs and repositories)
- Fast, no dependencies

**Example:**

```csharp
[Fact]
public void Expense_CreatedWithValidValues_ShouldHaveCorrectProperties()
{
    var expense = new Expense 
    { 
        Id = 1, Title = "Test", Amount = 50m, Category = "Testing", Date = DateTime.Now 
    };

    expense.Title.Should().Be("Test");
    expense.Amount.Should().Be(50m);
}
```

**Coverage:** 100% of domain entity properties.

---

#### Integration Tests — Repository (Real PostgreSQL)

**File:** `Integration/Repositories/ExpenseRepositoryTests.cs`

- Use **real PostgreSQL test container** (Testcontainers.PostgreSql)
- Verify raw ADO.NET SQL queries execute correctly
- Test `DataReader` → Entity mapping
- Verify database constraints and ordering (Date DESC)

**Why Real Database?** You use raw Npgsql with manual SQL queries. Mocking defeats the purpose—you must verify that SQL actually executes and mappings work.

**Example:**

```csharp
[Collection("Database collection")]
public class ExpenseRepositoryTests : IAsyncLifetime
{
    private readonly DatabaseFixture _fixture;
    private ExpenseRepository _repository;

    public ExpenseRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        var provider = new ConnectionProvider(_fixture.ConnectionString);
        _repository = new ExpenseRepository(provider);
    }

    [Fact]
    public async Task AddAsync_WithValidExpense_InsertsAndReturnsWithId()
    {
        var expense = new Expense 
        { 
            Title = "Test", Amount = 99.99m, Category = "Testing", Date = DateTime.Now 
        };

        var result = await _repository.AddAsync(expense);

        result.Id.Should().BeGreaterThan(0);
        result.Title.Should().Be("Test");
    }

    [Fact]
    public async Task GetAllAsync_WithMultipleExpenses_ReturnsOrderedByDateDesc()
    {
        var now = DateTime.Now;
        await _repository.AddAsync(new Expense { Title = "Old", Date = now.AddDays(-1) });
        await _repository.AddAsync(new Expense { Title = "New", Date = now });

        var result = (await _repository.GetAllAsync()).ToList();

        result.First().Title.Should().Be("New"); // Most recent first
        result.Should().BeInDescendingOrder(e => e.Date);
    }
}
```

**Coverage:** 85%+ of repository CRUD operations, ordering constraints, edge cases.

**Database Fixture:**

```csharp
public class DatabaseFixture : IAsyncLifetime
{
    private PostgreSqlContainer _container;

    public string ConnectionString { get; private set; }

    public async Task InitializeAsync()
    {
        _container = new PostgreSqlBuilder("postgres:16")
            .Build();

        await _container.StartAsync();
        ConnectionString = _container.GetConnectionString();

        // Create schema (tables, indexes)
        await InitializeDatabaseSchema();
    }

    public async Task DisposeAsync()
    {
        await _container.StopAsync();
        await _container.DisposeAsync();
    }
}
```

---

#### Integration Tests — Controllers (Mocked Services)

**File:** `Integration/Controllers/ExpensesControllerTests.cs`, `AuthControllerTests.cs`

- Mock services; test HTTP semantics only
- Verify status codes (200, 201, 204, 404, 401)
- Test authorization guards (`[Authorize]`)

**Example:**

```csharp
[Fact]
public async Task GetAll_WithValidExpenses_ReturnsOkWith200()
{
    var expenses = new List<ExpenseResponseDto>
    {
        new(1, "Lunch", 15.50m, "Food", DateTime.Now)
    };

    _mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(expenses);

    var result = await _controller.GetAll();

    var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    okResult.StatusCode.Should().Be(200);
}

[Fact]
public async Task GetById_WithInvalidId_ReturnsNotFound()
{
    _mockService.Setup(s => s.GetByIdAsync(999)).ReturnsAsync((ExpenseResponseDto?)null);

    var result = await _controller.GetById(999);

    result.Should().BeOfType<NotFoundResult>();
}
```

**Coverage:** 70%+ of endpoints, success paths, error scenarios (404, 401).

---

#### Authentication Tests

**File:** `Integration/Controllers/AuthControllerTests.cs`

- Test JWT token generation with correct issuer/audience/expiry
- Verify login with valid/invalid credentials
- Validate token structure (3-part JWT)

**Example:**

```csharp
[Fact]
public void Login_WithValidCredentials_ReturnsOkWithValidJwt()
{
    var jwtConfigMock = new Mock<IConfigurationSection>();
    jwtConfigMock.Setup(s => s["Key"]).Returns("test-secret-key-long-enough");
    jwtConfigMock.Setup(s => s["Issuer"]).Returns("test-issuer");
    jwtConfigMock.Setup(s => s["Audience"]).Returns("test-audience");

    _mockConfiguration.Setup(c => c["SingleUser:Username"]).Returns("admin");
    _mockConfiguration.Setup(c => c["SingleUser:Password"]).Returns("P@ssw0rd!");
    _mockConfiguration.Setup(c => c.GetSection("Jwt")).Returns(jwtConfigMock.Object);

    var result = _controller.Login(new AuthController.LoginRequest("admin", "P@ssw0rd!"));

    var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    var response = okResult.Value as AuthController.LoginResponse;

    response.Token.Split('.').Should().HaveCount(3); // Valid JWT structure
    response.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
}

[Fact]
public void Login_WithInvalidPassword_ReturnsUnauthorized()
{
    _mockConfiguration.Setup(c => c["SingleUser:Username"]).Returns("admin");
    _mockConfiguration.Setup(c => c["SingleUser:Password"]).Returns("P@ssw0rd!");

    var result = _controller.Login(new AuthController.LoginRequest("admin", "wrongpassword"));

    result.Should().BeOfType<UnauthorizedObjectResult>();
}
```

**Coverage:** Valid/invalid credentials, JWT format, expiry calculations.

---

### Running Tests

```bash
# Run all tests
dotnet test ExpenseTracker.sln

# Run only unit tests
dotnet test --filter "FullyQualifiedName~Unit"

# Run only integration tests
dotnet test --filter "FullyQualifiedName~Integration"

# Run specific test class
dotnet test --filter "FullyQualifiedName~ExpenseServiceTests"

# Run with verbose output
dotnet test --verbosity normal

# Run with code coverage
dotnet test /p:CollectCoverageMetrics=true
```

### Coverage Goals

| Layer | Target | Status |
| --- | --- | --- |
| **Domain** (Entities) | 100% | ✅ Fully covered |
| **Application** (Services) | 80%+ | ✅ Fully covered |
| **Infrastructure** (Repositories) | 85%+ | ✅ Fully covered |
| **API** (Controllers) | 70%+ | ✅ Fully covered |

### Test Libraries

| Library | Purpose |
| --- | --- |
| **xUnit** | Test runner (cleaner than NUnit, preferred in .NET) |
| **Moq** | Mock interfaces and services |
| **FluentAssertions** | Readable assertion syntax (`.Should().Be(...)`) |
| **Testcontainers.PostgreSql** | Spin up PostgreSQL in Docker for integration tests |

### TDD Workflow

Follow Red → Green → Refactor:

1. **Red:** Write failing test

```bash
dotnet test --filter "CreateExpenseAsync"
# FAIL: ExpenseService not yet implemented
```

1. **Green:** Write minimum code to pass

```csharp
public async Task<ExpenseResponseDto> CreateAsync(CreateExpenseDto dto)
{
    var expense = new Expense { /* map from dto */ };
    return await _repository.AddAsync(expense);
}
```

1. **Refactor:** Improve without breaking tests
   - Extract magic strings
   - Simplify logic
   - Improve naming

---

## Architecture Decisions

| Decision | Rationale |
| --- | --- |
| PostgreSQL (Npgsql driver) | Enterprise-ready, scalable, common in modern cloud stacks. Uses raw Npgsql driver (not EF Core) per exercise constraints |
| Raw ADO.NET in Repository | No ORM magic; manual SQL + manual mapping demonstrates clean data access layer separation and aligns with exercise requirements (no EF, Dapper, or Mediator) |
| No AutoMapper | One entity → trivial manual mapping; avoids opinionated dependency |
| `record` DTOs | Immutable value-based types; match DTO semantics exactly |
| Manual schema creation | Keeps Infrastructure layer simple; no migration framework dependency; SQL scripts provide clear schema documentation |
| BCrypt password hashing | BCrypt implemented from dev up to prod for secure password storage |
| JWT with Refresh Tokens | Stateless authentication with short-lived access tokens and refresh token rotation |
| Multi-user support | Extended from single-user MVP to support multiple users with user-expense relationships |
| Infrastructure → Domain only | Clean Architecture inversion: API wires DI, Infrastructure stays decoupled from Application |
| Centralized Exception Handling | ExceptionHandlingMiddleware for consistent error responses and logging across all endpoints |
| Test Suite (TDD) | Testing Strategy by Layer |
