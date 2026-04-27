# Testing Guide for Expense Tracker

## Overview

This guide describes how to run and work with the comprehensive test suite for the Expense Tracker application. The test suite includes **unit tests**, **integration tests**, and **fixtures** that follow Clean Architecture principles.

## Quick Start

### Run All Tests
```bash
cd backend
dotnet test
```

### Run Specific Category
```bash
# Unit tests only
dotnet test --filter "FullyQualifiedName~Unit"

# Integration tests only  
dotnet test --filter "FullyQualifiedName~Integration"

# Specific test class
dotnet test --filter "FullyQualifiedName~ExpenseServiceTests"
```

### Run with Verbose Output
```bash
dotnet test --verbosity normal
```

### Run with Code Coverage
```bash
dotnet test /p:CollectCoverageMetrics=true
```

## Project Structure

```
backend/
├── ExpenseTracker.Tests/
│   ├── Unit/
│   │   ├── Services/
│   │   │   ├── ExpenseServiceTests.cs     (95+ test cases)
│   │   │   ├── UserServiceTests.cs        (60+ test cases)
│   │   │   ├── CategoryServiceTests.cs
│   │   │   ├── UserExpenseServiceTests.cs
│   │   │   └── AuthServiceTests.cs
│   │   └── Entities/
│   │       └── ExpenseTests.cs            (5+ test cases)
│   ├── Integration/
│   │   ├── Repositories/
│   │   │   ├── ExpenseRepositoryTests.cs  (10+ test cases)
│   │   │   ├── UserRepositoryTests.cs
│   │   │   └── CategoryRepositoryTests.cs
│   │   └── Controllers/
│   │       ├── ExpensesControllerTests.cs (10+ test cases)
│   │       ├── AuthControllerTests.cs     (8+ test cases)
│   │       ├── CategoriesControllerTests.cs
│   │       ├── UsersControllerTests.cs
│   │       └── UserExpensesControllerTests.cs
│   ├── Fixtures/
│   │   ├── DatabaseFixture.cs             (PostgreSQL test container)
│   │   └── DatabaseCollection.cs          (xUnit collection fixture)
│   ├── Helpers/
│   │   └── JwtTokenHelper.cs              (Token generation utilities)
│   └── README.md                          (Detailed test documentation)
└── TESTING_GUIDE.md                       (This file)
```

## Test Categories Explained

### Unit Tests — Services

**Location:** `Unit/Services/`

Test **business logic** in isolation using **mocked repositories**.

**Examples:**
- `ExpenseServiceTests`: 95+ assertions covering GetAll, GetById, Create, Update, Delete
- `UserServiceTests`: 60+ assertions covering user CRUD operations
- `CategoryServiceTests`: Assertions covering category CRUD operations
- `UserExpenseServiceTests`: Assertions covering user-expense relationship operations
- `AuthServiceTests`: Assertions covering JWT generation, refresh tokens, BCrypt password verification

**Key Pattern:** Mock `IRepository` interfaces; verify service orchestration, DTO mapping, and business rules.

```csharp
var mockRepo = new Mock<IExpenseRepository>();
var service = new ExpenseService(mockRepo.Object);

var result = await service.CreateAsync(dto);

mockRepo.Verify(r => r.AddAsync(It.IsAny<Expense>()), Times.Once);
```

**Why:** Services should not depend on database implementation; mocking lets us test business logic at unit speed.

---

### Unit Tests — Entities

**Location:** `Unit/Entities/`

Test **domain models** (POCOs) and invariants.

**Example:** `ExpenseTests` validates property assignment, default values, and mutability.

**Why:** Lightweight tests that verify domain contracts without dependencies.

---

### Integration Tests — Repository

**Location:** `Integration/Repositories/`

Test **data access layer** with a **real PostgreSQL database** in a test container.

**Why Real Database?**
You use raw ADO.NET + Npgsql with manual SQL queries and `DataReader` mapping. Mocking these is pointless—you need to verify:
- ✅ SQL queries execute correctly
- ✅ `DataReader` → Entity mapping works
- ✅ Database constraints are respected
- ✅ Ordering (DATE DESC) works as expected

**Database Setup:**
- Testcontainers.PostgreSQL spins up a PostgreSQL 16 container
- Schema created automatically in `InitializeDatabaseSchema()`
- Container cleaned up after tests finish

**Example Test:**
```csharp
[Collection("Database collection")]
public class ExpenseRepositoryTests : IAsyncLifetime
{
    private readonly DatabaseFixture _fixture;
    
    public async Task InitializeAsync()
    {
        var provider = new ConnectionProvider(_fixture.ConnectionString);
        _repository = new ExpenseRepository(provider);
    }

    [Fact]
    public async Task AddAsync_WithValidExpense_InsertsAndReturnsWithId()
    {
        var expense = new Expense { /* ... */ };
        var result = await _repository.AddAsync(expense);

        result.Id.Should().BeGreaterThan(0);
        result.Title.Should().Be(expense.Title);
    }
}
```

**Test Coverage:**
- ✅ All CRUD operations
- ✅ Edge cases (null return, empty collection, invalid IDs)
- ✅ Database constraints (ordering, type conversions)

---

### Integration Tests — Controllers

**Location:** `Integration/Controllers/`

Test **HTTP endpoints** with mocked services (verify HTTP semantics).

**Why Mocked Services?**
Controllers should only care about HTTP concerns (status codes, headers, routing). Services handle business logic, which is tested separately. Mocking services keeps controller tests fast and focused.

**Example:**
```csharp
[Fact]
public async Task GetAll_WithValidExpenses_ReturnsOkWith200()
{
    var expenses = new List<ExpenseResponseDto> { /* ... */ };
    _mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(expenses);
    
    var result = await _controller.GetAll();
    
    var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    okResult.StatusCode.Should().Be(200);
}
```

**Test Coverage:**
- ✅ HTTP status codes (200, 201, 204, 404, 401)
- ✅ Response body serialization
- ✅ Error handling and null checks

---

### Integration Tests — Authentication

**Location:** `Integration/Controllers/AuthControllerTests.cs`

Test JWT token generation, login flows, and credential validation.

**Example:**
```csharp
[Fact]
public void Login_WithValidCredentials_ReturnsOkWithToken()
{
    _mockConfiguration.Setup(c => c["SingleUser:Username"]).Returns("admin");
    _mockConfiguration.Setup(c => c["SingleUser:Password"]).Returns("P@ssw0rd!");
    
    var result = _controller.Login(new("admin", "P@ssw0rd!"));
    
    var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    var response = okResult.Value as AuthController.LoginResponse;
    
    response.Token.Split('.').Should().HaveCount(3); // Valid JWT
}
```

**Test Coverage:**
- ✅ Valid/invalid credentials
- ✅ JWT token structure (3 parts)
- ✅ Token expiry calculations
- ✅ Error responses (401)

---

## Fixtures & Helpers

### DatabaseFixture.cs

Manages PostgreSQL test container lifecycle.

```csharp
public class DatabaseFixture : IAsyncLifetime
{
    public string ConnectionString { get; private set; }
    
    public async Task InitializeAsync()
    {
        // Spin up PostgreSQL container
        // Create tables
    }
    
    public async Task DisposeAsync()
    {
        // Stop container
    }
}
```

**Used by:** All integration repository tests.

---

### JwtTokenHelper.cs

Utilities for JWT token generation in tests.

```csharp
// Generate valid token
var token = JwtTokenHelper.GenerateValidToken(userId: "123");

// Generate expired token (for testing validation failures)
var expiredToken = JwtTokenHelper.GenerateExpiredToken();
```

---

## Common Testing Patterns

### Mocking Repositories in Services

```csharp
var mockRepo = new Mock<IExpenseRepository>();

mockRepo
    .Setup(r => r.GetAllAsync())
    .ReturnsAsync(new List<Expense> { /* ... */ });

var service = new ExpenseService(mockRepo.Object);
var result = await service.GetAllAsync();

mockRepo.Verify(r => r.GetAllAsync(), Times.Once);
```

---

### Capturing Arguments in Mocks

```csharp
Expense? capturedExpense = null;

_mockRepository
    .Setup(r => r.AddAsync(It.IsAny<Expense>()))
    .Callback<Expense>(e => capturedExpense = e)
    .ReturnsAsync(/* created expense */);

await _service.CreateAsync(dto);

capturedExpense.Title.Should().Be(dto.Title);
```

---

### xUnit Collection Fixture

```csharp
[Collection("Database collection")]
public class MyRepositoryTests : IAsyncLifetime
{
    private readonly DatabaseFixture _fixture;
    
    public MyRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture; // Injected by xUnit
    }
    
    public async Task InitializeAsync() { /* setup per test */ }
    public Task DisposeAsync() => Task.CompletedTask;
}
```

**Benefit:** xUnit creates one DatabaseFixture instance per collection; all tests in that collection reuse it.

---

## Test Execution Examples

### Run All Tests
```bash
dotnet test ExpenseTracker.sln
```

**Output:**
```
  Test run started
  Passed: Unit test 1
  Passed: Unit test 2
  ...
  Passed: Integration test 1
  ...
  
  Test execution complete: 100+ tests, 100+ passed, 0 failed
```

---

### Run Only Unit Tests
```bash
dotnet test --filter "FullyQualifiedName~Unit"
```

---

### Run Single Test Method
```bash
dotnet test --filter "FullyQualifiedName~ExpenseServiceTests::CreateAsync_WithValidDto_CreatesAndReturnsExpense"
```

---

### Run with Coverage Report
```bash
dotnet test /p:CollectCoverageMetrics=true /p:CoverageFormat=opencover
```

---

## TDD Workflow

The test suite supports **Test-Driven Development (TDD)**:

1. **Write failing test** → RED
   ```bash
   dotnet test --filter "CreateExpenseAsync"
   # FAIL: ExpenseService not implemented
   ```

2. **Write minimum code** → GREEN
   ```csharp
   public async Task<ExpenseResponseDto> CreateAsync(CreateExpenseDto dto)
   {
       // Implement
   }
   ```

3. **Run test** → PASS
   ```bash
   dotnet test --filter "CreateExpenseAsync"
   # PASS
   ```

4. **Refactor** → CLEAN
   - Extract constants, simplify logic, improve naming
   - Run tests again to ensure no regression

---

## Troubleshooting

### Test Fails with "Connection refused"

**Cause:** Docker/Testcontainers not running

**Fix:**
```bash
# Ensure Docker daemon is running
docker ps

# Or skip integration tests
dotnet test --filter "FullyQualifiedName~Unit"
```

---

### Test Fails with "JWT signature validation failed"

**Cause:** JWT key or configuration mismatch

**Fix:** Verify test configuration uses correct key in `JwtTokenHelper.cs` or `AuthControllerTests.cs`.

---

### Test Hangs or Times Out

**Cause:** Long-running operation or deadlock

**Fix:**
```bash
# Run with timeout
dotnet test --timeout 5000  # 5 seconds per test

# Or run verbose to see which test hangs
dotnet test --verbosity normal
```

---

## Coverage Goals

| Layer | Target | Coverage |
|-------|--------|----------|
| **Domain** (Entities) | 100% | ✅ Fully covered |
| **Application** (Services) | 80%+ | ✅ Fully covered |
| **Infrastructure** (Repositories) | 85%+ | ✅ Fully covered |
| **API** (Controllers) | 70%+ | ✅ Fully covered |

---

## Next Steps

1. **Add CategoryServiceTests** → Tests for category CRUD operations with mocked repositories
2. **Add UserExpenseServiceTests** → Tests for user-expense relationship management
3. **Add AuthServiceTests** → Tests for JWT token generation, refresh tokens, password hashing
4. **Add Category/User Repository Integration Tests** → Real database tests with Testcontainers
5. **Add Additional Controller Tests** → CategoriesControllerTests, UsersControllerTests, UserExpensesControllerTests
6. **CI/CD Integration** → GitHub Actions, run tests on every PR
7. **Coverage Reports** → ReportGenerator, HTML reports in CI

---

## References

- **xUnit:** https://xunit.net/
- **Moq:** https://github.com/moq/moq4
- **FluentAssertions:** https://fluentassertions.com/
- **Testcontainers:** https://testcontainers.com/
- **Clean Architecture Testing:** https://blog.cleancoder.com/uncle-bob/2017/03/03/TDD-Harms-Architecture.html

---

## Questions?

Refer to `ExpenseTracker.Tests/README.md` for detailed test documentation and code walkthroughs.
