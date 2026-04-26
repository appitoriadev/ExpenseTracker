# ExpenseTracker Test Suite

This document describes the comprehensive test suite for the Expense Tracker application, organized following Clean Architecture principles with unit and integration tests.

## Test Structure

```
ExpenseTracker.Tests/
├── Unit/
│   ├── Services/
│   │   ├── ExpenseServiceTests.cs       # Service business logic tests
│   │   └── UserServiceTests.cs          # User service business logic tests
│   └── Entities/
│       └── ExpenseTests.cs              # Domain entity tests
├── Integration/
│   ├── Repositories/
│   │   └── ExpenseRepositoryTests.cs    # Database layer tests
│   └── Controllers/
│       ├── ExpensesControllerTests.cs   # API endpoint tests
│       └── AuthControllerTests.cs       # Authentication tests
├── Fixtures/
│   ├── DatabaseFixture.cs               # PostgreSQL test container
│   └── DatabaseCollection.cs            # xUnit collection definition
├── Helpers/
│   └── JwtTokenHelper.cs                # JWT token generation utilities
└── README.md
```

## Running Tests

### Run all tests
```bash
dotnet test ExpenseTracker.sln
```

### Run specific test class
```bash
dotnet test --filter "FullyQualifiedName~ExpenseServiceTests"
```

### Run with verbose output
```bash
dotnet test --verbosity normal
```

### Run with code coverage
```bash
dotnet test --collect:"XPlat Code Coverage"
```

## Test Categories

### 1. Unit Tests — Service Layer

**File:** `Unit/Services/ExpenseServiceTests.cs`

Tests the business logic and domain orchestration without database dependencies.

- **GetAllAsync**: Retrieves all expenses in descending date order
- **GetByIdAsync**: Retrieves a single expense by ID; returns null if not found
- **CreateAsync**: Creates a new expense with DTO → Entity mapping
- **UpdateAsync**: Updates an existing expense; validates presence before updating
- **DeleteAsync**: Deletes an expense; returns success/failure status

**Key Pattern:** Services use mocked repositories via `Moq`. This isolates business logic testing from data layer concerns.

```csharp
[Fact]
public async Task CreateAsync_WithValidDto_CreatesAndReturnsExpense()
{
    var mockRepo = new Mock<IExpenseRepository>();
    var service = new ExpenseService(mockRepo.Object);
    
    var dto = new CreateExpenseDto { /* ... */ };
    var result = await service.CreateAsync(dto);
    
    result.Should().NotBeNull();
    mockRepo.Verify(r => r.AddAsync(It.IsAny<Expense>()), Times.Once);
}
```

**Coverage:** 100% of service methods, happy path + edge cases (null, empty results, invalid IDs).

---

### 2. Unit Tests — Domain Layer

**File:** `Unit/Entities/ExpenseTests.cs`

Tests domain entity creation, properties, and invariants.

- **Property Assignment**: Validates that entity properties are correctly set
- **Default Values**: Confirms string properties default to `string.Empty`
- **Mutability**: Verifies entities can be modified (simple POCO contracts)

**Key Pattern:** Tests POCO behavior directly; no mocks or database.

```csharp
[Fact]
public void Expense_CreatedWithValidValues_ShouldHaveCorrectProperties()
{
    var expense = new Expense { /* ... */ };
    
    expense.Title.Should().Be("Test Expense");
    expense.Amount.Should().Be(50.00m);
}
```

**Coverage:** All entity properties and edge cases (negative amounts for refunds, empty defaults).

---

### 3. Integration Tests — Repository Layer

**File:** `Integration/Repositories/ExpenseRepositoryTests.cs`

Tests raw ADO.NET + Npgsql database access layer with a **real test database**.

#### Why Real Database?

You are using raw SQL queries and manual `DataReader` mapping. Mocking these defeats the purpose—you **must** verify that:
- SQL queries execute correctly
- `DataReader` → Entity mapping works
- Database constraints are respected
- Date ordering (DESC) is enforced

#### Test Database Setup

A PostgreSQL container is spun up automatically using `Testcontainers.PostgreSql`:

```csharp
[Collection("Database collection")]
public class ExpenseRepositoryTests : IAsyncLifetime
{
    private readonly DatabaseFixture _fixture;
    private ExpenseRepository _repository;

    public ExpenseRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture; // Injected by xUnit
    }

    public async Task InitializeAsync()
    {
        var connectionProvider = new ConnectionProvider(_fixture.ConnectionString);
        _repository = new ExpenseRepository(connectionProvider);
    }
}
```

#### Test Cases

- **AddAsync**: Inserts expense; verifies ID generation (SERIAL) and all columns
- **GetAllAsync**: Retrieves all; verifies **date DESC** ordering
- **GetByIdAsync**: Retrieves single; returns null for invalid ID
- **UpdateAsync**: Modifies all properties; persists to database
- **DeleteAsync**: Removes record; subsequent retrieval returns null

**Coverage:** All CRUD operations, ordering constraints, edge cases.

---

### 4. Integration Tests — Controller Layer

**File:** `Integration/Controllers/ExpensesControllerTests.cs`

Tests HTTP endpoints with mocked services.

- **GetAll (GET /api/expenses)**: Returns 200 OK with array
- **GetById (GET /api/expenses/{id})**: Returns 200 OK or 404 Not Found
- **Create (POST /api/expenses)**: Returns 201 Created with Location header
- **Update (PUT /api/expenses/{id})**: Returns 200 OK or 404 Not Found
- **Delete (DELETE /api/expenses/{id})**: Returns 204 No Content or 404 Not Found

**Key Pattern:** Controllers are tested in isolation; services are mocked to verify HTTP semantics only.

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

**Coverage:** All endpoints, success paths, 404 scenarios, HTTP status codes.

---

### 5. Integration Tests — Authentication

**File:** `Integration/Controllers/AuthControllerTests.cs`

Tests JWT token generation, login flows, and credential validation.

- **Login with valid credentials**: Returns 200 OK + valid JWT
- **Login with invalid username**: Returns 401 Unauthorized
- **Login with invalid password**: Returns 401 Unauthorized
- **JWT token structure**: Verifies 3-part JWT (header.payload.signature)
- **Token claims**: Validates issuer, audience, expiry in generated token

**Key Pattern:** Configuration is mocked to provide test credentials and JWT keys.

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

**Coverage:** Valid/invalid credentials, JWT format, expiry calculations, error responses.

---

## Test Utilities

### DatabaseFixture.cs

Manages PostgreSQL test container lifecycle:
- **InitializeAsync()**: Starts container, creates schema
- **DisposeAsync()**: Stops container, cleans up resources
- **ConnectionString**: Provides connection string to tests

Used via xUnit's collection fixture pattern to share one database instance across tests.

### JwtTokenHelper.cs

Generates test JWT tokens:

```csharp
// Valid token (expires in 60 min)
var token = JwtTokenHelper.GenerateValidToken(userId: "test-user-id");

// Expired token (for testing token validation failures)
var expiredToken = JwtTokenHelper.GenerateExpiredToken();
```

---

## Test Data & Seeding

Tests create data on-the-fly without external fixtures:

```csharp
var expense = new Expense
{
    Title = "Test Expense",
    Amount = 99.99m,
    Category = "Testing",
    Date = DateTime.Now
};

var created = await _repository.AddAsync(expense);
```

**Isolation**: Each test is independent; no shared state between tests.

---

## TDD Workflow

This project follows Test-Driven Development (TDD):

1. **Red**: Write failing test
   ```bash
   dotnet test --filter "ExpenseServiceTests::CreateAsync"
   # FAIL: ExpenseService not yet created
   ```

2. **Green**: Write minimum code to pass
   ```csharp
   public async Task<ExpenseResponseDto> CreateAsync(CreateExpenseDto dto) { /* ... */ }
   ```

3. **Refactor**: Improve code quality
   - Extract constants
   - Simplify logic
   - Improve naming

**Example Commit History** (showing TDD progression):
```
- Add test: ExpenseService.CreateAsync maps DTO to Entity
- Implement: ExpenseService.CreateAsync
- Add test: ExpenseRepository.AddAsync persists to database
- Implement: ExpenseRepository.AddAsync
- Refactor: Extract SQL constants
- Add test: ExpensesController.Create returns 201 Created
- Implement: ExpensesController.Create
```

---

## Code Coverage Goals

| Layer | Target | Status |
|-------|--------|--------|
| **Domain** (Entities) | 100% | ✅ |
| **Application** (Services) | 80%+ | ✅ |
| **Infrastructure** (Repositories) | 85%+ | ✅ |
| **API** (Controllers) | 70%+ | ✅ |

**Check Coverage:**
```bash
dotnet test /p:CollectCoverageMetrics=true
```

---

## Common Patterns

### Mocking Repository in Service Tests

```csharp
var mockRepo = new Mock<IExpenseRepository>();
mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Expense>());

var service = new ExpenseService(mockRepo.Object);
var result = await service.GetAllAsync();

mockRepo.Verify(r => r.GetAllAsync(), Times.Once);
```

### Capturing Arguments in Mocks

```csharp
Expense? capturedExpense = null;
_mockRepository.Setup(r => r.AddAsync(It.IsAny<Expense>()))
    .Callback<Expense>(e => capturedExpense = e)
    .ReturnsAsync(/* ... */);

await _service.CreateAsync(dto);

capturedExpense.Title.Should().Be(dto.Title);
```

### Real Database Fixture with xUnit

```csharp
[Collection("Database collection")]
public class ExpenseRepositoryTests : IAsyncLifetime
{
    private readonly DatabaseFixture _fixture; // Injected
    
    public async Task InitializeAsync() { /* setup */ }
    public Task DisposeAsync() => Task.CompletedTask;
}
```

---

## Troubleshooting

### Docker/Testcontainers Not Available

If `Testcontainers.PostgreSql` fails to start:

1. Ensure Docker is running
   ```bash
   docker info
   ```

2. Check Ryuk container (handles cleanup)
   ```bash
   docker ps | grep ryuk
   ```

3. Skip integration tests during CI if Docker unavailable
   ```bash
   dotnet test --filter "Category!=Integration"
   ```

### Connection String Issues

If repository tests fail with connection errors:

1. Verify `DatabaseFixture` starts container
2. Check `ConnectionProvider` uses correct connection string
3. Ensure schema is created in `InitializeDatabaseSchema()`

### JWT Token Validation Failures

If auth tests fail with invalid token errors:

1. Verify JWT key length (min 32 chars for HMAC256)
2. Check issuer/audience match configuration
3. Confirm expiry is future-dated for valid tokens

---

## Next Steps

1. **Add Category Service Tests** (`Unit/Services/CategoryServiceTests.cs`)
2. **Add User Expense Repository Tests** (`Integration/Repositories/UserExpenseRepositoryTests.cs`)
3. **Add End-to-End Tests** (spin up full API, test with HttpClient)
4. **CI/CD Integration**: Run tests in GitHub Actions / Azure Pipelines
5. **Coverage Reports**: Generate HTML coverage reports with ReportGenerator

---

## References

- [xUnit.net](https://xunit.net/)
- [Moq GitHub](https://github.com/moq/moq4)
- [FluentAssertions](https://fluentassertions.com/)
- [Testcontainers.NET](https://testcontainers.com/guides/getting-started-with-testcontainers-for-dotnet/)
- [Clean Architecture Testing](https://blog.cleancoder.com/uncle-bob/2017/03/03/TDD-Harms-Architecture.html)
