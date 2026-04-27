# Expense Tracker Backend - Comprehensive Recreation Prompt

## Overview
Act as a Senior .NET Architect. Create a C# .NET 10 Web API for an Expense Tracker application using **Clean Architecture** with **raw ADO.NET + Npgsql** (no EF Core/Dapper). The API will be consumed by a React frontend.

**Architecture:** Domain → Application → Infrastructure + API (all layers with clear dependency inversion)

---

## Phase 1: Solution & Project Scaffolding

```bash
# Create solution
dotnet new sln -n ExpenseTracker

# Create projects (net10.0)
dotnet new classlib -n ExpenseTracker.Domain -f net10.0 -o ExpenseTracker.Domain
dotnet new classlib -n ExpenseTracker.Application -f net10.0 -o ExpenseTracker.Application
dotnet new classlib -n ExpenseTracker.Infrastructure -f net10.0 -o ExpenseTracker.Infrastructure
dotnet new webapi -n ExpenseTracker.Api -f net10.0 -o ExpenseTracker.Api
dotnet new xunit -n ExpenseTracker.Tests -f net10.0 -o ExpenseTracker.Tests

# Add to solution
dotnet sln ExpenseTracker.sln add \
  ExpenseTracker.Domain/ExpenseTracker.Domain.csproj \
  ExpenseTracker.Application/ExpenseTracker.Application.csproj \
  ExpenseTracker.Infrastructure/ExpenseTracker.Infrastructure.csproj \
  ExpenseTracker.Api/ExpenseTracker.Api.csproj \
  ExpenseTracker.Tests/ExpenseTracker.Tests.csproj

# Project references (clean dependency inversion)
dotnet add ExpenseTracker.Application/ExpenseTracker.Application.csproj \
    reference ExpenseTracker.Domain/ExpenseTracker.Domain.csproj

dotnet add ExpenseTracker.Infrastructure/ExpenseTracker.Infrastructure.csproj \
    reference ExpenseTracker.Domain/ExpenseTracker.Domain.csproj

dotnet add ExpenseTracker.Api/ExpenseTracker.Api.csproj \
    reference ExpenseTracker.Application/ExpenseTracker.Application.csproj \
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
    package Microsoft.AspNetCore.Authentication.JwtBearer \
    package Swashbuckle.AspNetCore \
    package dotenv.net

dotnet add ExpenseTracker.Tests/ExpenseTracker.Tests.csproj \
    package Moq \
    package FluentAssertions \
    package Testcontainers.PostgreSql
```

---

## Phase 2: Domain Layer (Zero External Dependencies)

### Entities

**`Entities/Expense.cs`**
```
Properties: Id (Guid), Title (string), Amount (decimal), CategoryName (string), Date (DateTime)
```

**`Entities/Category.cs`**
```
Properties: Id (Guid), CategoryName (string)
```

**`Entities/User.cs`**
```
Properties: Id (Guid), Username (string), PasswordHash (string), FirstName (string), LastName (string), Email (string), RefreshToken (string?), RefreshTokenExpiry (DateTime?)
```

**`Entities/UserExpense.cs`**
```
Properties: Id (Guid), ExpensesId (Guid), UserId (Guid)
```

### Interfaces (All async methods)

**`Interfaces/IExpenseRepository.cs`**
- GetAllAsync() → IEnumerable<Expense>
- GetByIdAsync(Guid id) → Expense?
- AddAsync(Expense expense) → Expense
- UpdateAsync(Guid id, Expense expense) → Expense?
- DeleteAsync(Guid id) → bool

**`Interfaces/ICategoryRepository.cs`**
- GetAllAsync() → IEnumerable<Category>
- GetByIdAsync(Guid id) → Category?
- GetByNameAsync(string categoryName) → Category?
- AddAsync(Category category) → Category
- UpdateAsync(Guid id, Category category) → Category?
- DeleteAsync(Guid id) → bool

**`Interfaces/IUserRepository.cs`**
- GetAllAsync() → IEnumerable<User>
- GetByIdAsync(Guid id) → User?
- GetByUsernameAsync(string username) → User?
- GetByEmailAsync(string email) → User?
- AddAsync(User user) → User
- UpdateAsync(Guid id, User user) → User?
- DeleteAsync(Guid id) → bool

**`Interfaces/IUserExpenseRepository.cs`**
- GetAllAsync() → IEnumerable<UserExpense>
- GetByIdAsync(Guid id) → UserExpense?
- GetByUserIdAsync(Guid userId) → IEnumerable<UserExpense>
- AddAsync(UserExpense userExpense) → UserExpense
- DeleteAsync(Guid id) → bool

---

## Phase 3: Application Layer (DTOs + Services + Logging)

### DTOs (C# record types - immutable)

**ExpenseDto variants:**
- `CreateExpenseDto(Title, Amount, CategoryName, Date)`
- `UpdateExpenseDto(Title, Amount, CategoryName, Date)`
- `ExpenseResponseDto(Id, Title, Amount, CategoryName, Date)`

**CategoryDto variants:**
- `CreateCategoryDto(CategoryName)`
- `UpdateCategoryDto(CategoryName)`
- `CategoryDto(Id, CategoryName)`

**UserDto variants:**
- `CreateUserDto(Username, Password, FirstName, LastName, Email)`
- `UpdateUserDto(FirstName, LastName, Email)`
- `UserResponseDto(Id, Username, FirstName, LastName, Email)`

**AuthDto variants:**
- `LoginRequest(Username, Password)`
- `RegisterRequest(Username, Password, FirstName, LastName, Email)`
- `AuthResponse(Token, RefreshToken, ExpiresAt, Message?)`

**UserExpenseDto variants:**
- `CreateUserExpenseDto(ExpensesId, UserId)`
- `UserExpenseDto(Id, ExpensesId, UserId)`

### Service Interfaces (All async)

**`Interfaces/IExpenseService.cs`**
- GetAllAsync() → IEnumerable<ExpenseResponseDto>
- GetByIdAsync(Guid) → ExpenseResponseDto?
- CreateAsync(CreateExpenseDto) → ExpenseResponseDto
- UpdateAsync(Guid, UpdateExpenseDto) → ExpenseResponseDto?
- DeleteAsync(Guid) → bool

**`Interfaces/ICategoryService.cs`**
- GetAllAsync() → IEnumerable<CategoryDto>
- GetByIdAsync(Guid) → CategoryDto?
- GetByNameAsync(string) → CategoryDto?
- CreateAsync(CreateCategoryDto) → CategoryDto
- UpdateAsync(Guid, UpdateCategoryDto) → CategoryDto?
- DeleteAsync(Guid) → bool

**`Interfaces/IUserService.cs`**
- GetAllAsync() → IEnumerable<UserResponseDto>
- GetByIdAsync(Guid) → UserResponseDto?
- GetByUsernameAsync(string) → UserResponseDto?
- CreateAsync(CreateUserDto) → UserResponseDto
- UpdateAsync(Guid, UpdateUserDto) → UserResponseDto?
- DeleteAsync(Guid) → bool

**`Interfaces/IUserExpenseService.cs`**
- GetAllAsync() → IEnumerable<UserExpenseDto>
- GetByUserIdAsync(Guid userId) → IEnumerable<UserExpenseDto>
- AddAsync(CreateUserExpenseDto) → UserExpenseDto
- DeleteAsync(Guid) → bool

**`Interfaces/IAuthService.cs`**
- RegisterAsync(RegisterRequest) → AuthResponse
- LoginAsync(LoginRequest) → AuthResponse
- RefreshTokenAsync(string refreshToken) → AuthResponse?
- ValidatePasswordAsync(string password, string hash) → bool

### Service Implementations

**`Services/ExpenseService.cs`**
- Depends on: IExpenseRepository, ICategoryService, ILogger<ExpenseService>
- Maps: Expense ↔ ExpenseResponseDto
- Validates: Title (required, 1-255), Amount (>0, ≤999999.99), CategoryName (required)
- Logging: Log all operations (GetAll, GetById, Create, Update, Delete)
- Error handling: ArgumentException for validation, ArgumentNullException for null DTO

**`Services/CategoryService.cs`**
- Depends on: ICategoryRepository, ILogger<CategoryService>
- Maps: Category ↔ CategoryDto
- Validates: CategoryName (required, 1-255, unique)
- Logging: Log all operations
- Error handling: InvalidOperationException for duplicate category names

**`Services/UserService.cs`**
- Depends on: IUserRepository, ILogger<UserService>
- Maps: User ↔ UserResponseDto
- Validates: Username (unique), Email (unique, valid format), FirstName/LastName (required)
- Logging: Log all operations
- Error handling: InvalidOperationException for duplicates

**`Services/UserExpenseService.cs`**
- Depends on: IUserExpenseRepository, IExpenseRepository, IUserRepository, ILogger<UserExpenseService>
- Maps: UserExpense ↔ UserExpenseDto
- Validates: ExpensesId and UserId exist (foreign key checks)
- Logging: Log all operations
- Error handling: ArgumentException for invalid IDs

**`Services/AuthService.cs`**
- Depends on: IUserRepository, IConfiguration, ILogger<AuthService>
- **RegisterAsync(RegisterRequest):**
  - Hash password using BCrypt
  - Generate refresh token (Guid + expiry 7 days)
  - Insert user into repository
  - Generate JWT (1-hour expiry)
  - Return AuthResponse with both tokens
- **LoginAsync(LoginRequest):**
  - Fetch user by username
  - Verify password using BCrypt
  - Generate new JWT
  - Generate new refresh token
  - Update user's refresh token in repository
  - Return AuthResponse
- **RefreshTokenAsync(string refreshToken):**
  - Find user by refresh token
  - Validate token expiry
  - Generate new JWT
  - Generate new refresh token
  - Update user's refresh token
  - Return AuthResponse
- **Validation Error Handling:** InvalidOperationException for "Username/Email exists" or "Invalid credentials"
- **Logging:** Log register, login, refresh, and password validation failures

---

## Phase 4: Infrastructure Layer (Raw ADO.NET)

### `Data/ConnectionProvider.cs`
- Singleton service wrapping NpgsqlConnection
- Constructor takes connection string from environment or appsettings
- Property: `string ConnectionString { get; }`
- Method: `NpgsqlConnection GetConnection()` → Returns new connection with ConnectionString set

### Repository Implementations (All async, manual SQL + DataReader mapping)

**`Repositories/ExpenseRepository.cs`**
- Implements: IExpenseRepository
- **GetAllAsync():** SELECT * FROM dbo.expenses ORDER BY date DESC
- **GetByIdAsync(Guid):** SELECT * FROM dbo.expenses WHERE id = @id
- **AddAsync(Expense):** INSERT INTO dbo.expenses (...) VALUES (...); SELECT LAST_INSERT_ID()
- **UpdateAsync(Guid, Expense):** UPDATE dbo.expenses SET ... WHERE id = @id; SELECT *
- **DeleteAsync(Guid):** DELETE FROM dbo.expenses WHERE id = @id
- **DataReader Mapping:** Expense { Id, Title, Amount, CategoryName, Date }
- Uses: NpgsqlConnection, NpgsqlCommand, NpgsqlDataReader

**`Repositories/CategoryRepository.cs`**
- Implements: ICategoryRepository
- **GetAllAsync():** SELECT * FROM dbo.categories ORDER BY category_name ASC
- **GetByIdAsync(Guid):** SELECT * FROM dbo.categories WHERE id = @id
- **GetByNameAsync(string):** SELECT * FROM dbo.categories WHERE category_name = @categoryName
- **AddAsync(Category):** INSERT INTO dbo.categories (id, category_name) VALUES (@id, @categoryName); SELECT *
- **UpdateAsync(Guid, Category):** UPDATE dbo.categories SET category_name = @categoryName WHERE id = @id
- **DeleteAsync(Guid):** DELETE FROM dbo.categories WHERE id = @id
- **DataReader Mapping:** Category { Id, CategoryName }

**`Repositories/UserRepository.cs`**
- Implements: IUserRepository
- **GetAllAsync():** SELECT * FROM dbo.users
- **GetByIdAsync(Guid):** SELECT * FROM dbo.users WHERE id = @id
- **GetByUsernameAsync(string):** SELECT * FROM dbo.users WHERE username = @username
- **GetByEmailAsync(string):** SELECT * FROM dbo.users WHERE email = @email
- **AddAsync(User):** INSERT INTO dbo.users (...) VALUES (...); SELECT *
- **UpdateAsync(Guid, User):** UPDATE dbo.users SET ... WHERE id = @id; SELECT *
- **DeleteAsync(Guid):** DELETE FROM dbo.users WHERE id = @id
- **DataReader Mapping:** User { Id, Username, PasswordHash, FirstName, LastName, Email, RefreshToken, RefreshTokenExpiry }

**`Repositories/UserExpenseRepository.cs`**
- Implements: IUserExpenseRepository
- **GetAllAsync():** SELECT * FROM dbo.user_expenses
- **GetByIdAsync(Guid):** SELECT * FROM dbo.user_expenses WHERE id = @id
- **GetByUserIdAsync(Guid):** SELECT * FROM dbo.user_expenses WHERE user_id = @userId
- **AddAsync(UserExpense):** INSERT INTO dbo.user_expenses (...) VALUES (...); SELECT *
- **DeleteAsync(Guid):** DELETE FROM dbo.user_expenses WHERE id = @id
- **DataReader Mapping:** UserExpense { Id, ExpensesId, UserId }

### `Data/Schema.sql` (PostgreSQL initialization)

```sql
CREATE DATABASE ExpenseTracker;
CREATE SCHEMA IF NOT EXISTS dbo;

-- Categories table
CREATE TABLE IF NOT EXISTS dbo.categories (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    category_name VARCHAR(255) NOT NULL UNIQUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Expenses table (FK to categories)
CREATE TABLE IF NOT EXISTS dbo.expenses (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    title VARCHAR(255) NOT NULL,
    amount NUMERIC(18, 2) NOT NULL CHECK (amount > 0),
    category_name VARCHAR(255) NOT NULL,
    date TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_categories FOREIGN KEY (category_name) REFERENCES dbo.categories (category_name)
);

-- Users table (with refresh token fields)
CREATE TABLE IF NOT EXISTS dbo.users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    username VARCHAR(255) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    firstname VARCHAR(255) NOT NULL,
    lastname VARCHAR(255) NOT NULL,
    email VARCHAR(255) UNIQUE NOT NULL,
    refresh_token VARCHAR(512),
    refresh_token_expiry TIMESTAMP,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- User-Expenses junction table
CREATE TABLE IF NOT EXISTS dbo.user_expenses (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    expenses_id UUID NOT NULL,
    user_id UUID NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_user FOREIGN KEY (user_id) REFERENCES dbo.users (id),
    CONSTRAINT fk_expenses FOREIGN KEY (expenses_id) REFERENCES dbo.expenses (id)
);

-- Indexes
CREATE INDEX IF NOT EXISTS idx_expenses_date ON dbo.expenses (date DESC);
CREATE INDEX IF NOT EXISTS idx_users_username ON dbo.users (username);
CREATE INDEX IF NOT EXISTS idx_userexpenses_user_id ON dbo.user_expenses (user_id);
CREATE INDEX IF NOT EXISTS idx_userexpenses_expense_id ON dbo.user_expenses (expenses_id);
```

---

## Phase 5: API Layer (Controllers + Middleware + Configuration)

### `Middleware/ExceptionHandlingMiddleware.cs`
- Catches all exceptions in request pipeline
- Logs exceptions with ILogger
- Returns JSON error response: `{ message: string, statusCode: int, timestamp: DateTime }`
- Maps exception types to HTTP status codes:
  - ArgumentException / ArgumentNullException → 400 Bad Request
  - InvalidOperationException → 409 Conflict
  - Exception → 500 Internal Server Error

### Controllers (All [Authorize], [ApiController], [Route("api/[controller]")])

**`Controllers/AuthController.cs`** (NO [Authorize])
- POST /api/auth/register
  - Body: RegisterRequest
  - Response: 200 AuthResponse
  - Errors: 400 (invalid data), 409 (username/email exists)
- POST /api/auth/login
  - Body: LoginRequest
  - Response: 200 AuthResponse
  - Errors: 400 (missing data), 401 (invalid credentials)
- POST /api/auth/refresh
  - Body: { refreshToken: string }
  - Response: 200 AuthResponse
  - Errors: 401 (invalid/expired refresh token)

**`Controllers/ExpensesController.cs`** ([Authorize])
- GET / → 200 IEnumerable<ExpenseResponseDto>
- GET /{id:Guid} → 200 ExpenseResponseDto | 404
- POST / → 201 ExpenseResponseDto | 400
- PUT /{id:Guid} → 200 ExpenseResponseDto | 404
- DELETE /{id:Guid} → 204 | 404

**`Controllers/CategoriesController.cs`** ([Authorize])
- GET / → 200 IEnumerable<CategoryDto>
- GET /{id:Guid} → 200 CategoryDto | 404
- POST / → 201 CategoryDto | 400
- PUT /{id:Guid} → 200 CategoryDto | 404
- DELETE /{id:Guid} → 204 | 404

**`Controllers/UsersController.cs`** ([Authorize])
- GET / → 200 IEnumerable<UserResponseDto>
- GET /{id:Guid} → 200 UserResponseDto | 404
- POST / (admin only) → 201 UserResponseDto | 400
- PUT /{id:Guid} → 200 UserResponseDto | 404
- DELETE /{id:Guid} → 204 | 404

**`Controllers/UserExpensesController.cs`** ([Authorize])
- GET / → 200 IEnumerable<UserExpenseDto>
- GET /user/{userId:Guid} → 200 IEnumerable<UserExpenseDto>
- POST / → 201 UserExpenseDto | 400
- DELETE /{id:Guid} → 204 | 404

### `Program.cs` Configuration

**DI Registration:**
- ConnectionProvider (Singleton)
- All Repositories (Scoped)
- All Services (Scoped)
- Logging, HttpContextAccessor

**JWT Configuration (from .env):**
- JWT_KEY (minimum 32 characters)
- JWT_ISSUER
- JWT_AUDIENCE
- Token validation: Issuer, Audience, Lifetime, SigningKey (HS256)
- Expiry: 1 hour for access token

**CORS:**
- Policy: "AllowAll" or "ReactFrontend"
- Allowed Origins: From appsettings:Cors:AllowedOrigins (localhost:5173, localhost:3000)
- AllowAnyHeader, AllowAnyMethod

**Swagger:**
- Title: "Expense Tracker REST API"
- Description: "REST API for expense tracking with user authentication"
- Bearer token support with JWT format input field

**Middleware Pipeline Order:**
1. ExceptionHandlingMiddleware
2. Swagger (dev only)
3. HTTPS Redirect
4. CORS
5. Authentication
6. Authorization
7. Controllers

### `appsettings.json`
```json
{
  "Jwt": {
    "Key": "your-256-bit-secret-key-here-minimum-32-chars",
    "Issuer": "ExpenseTrackerIssuer",
    "Audience": "ExpenseTrackerAudience",
    "ExpiryMinutes": 60
  },
  "Cors": {
    "AllowedOrigins": ["http://localhost:5173", "http://localhost:3000"]
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  }
}
```

### `.env.local` (Local development)
```
CONNECTIONSTRINGS_EXPENSETRACKER=Host=localhost;Port=5432;Database=ExpenseTracker;Username=postgres;Password=postgres
JWT_KEY=your-256-bit-secret-key-here-minimum-32-chars
JWT_ISSUER=ExpenseTrackerIssuer
JWT_AUDIENCE=ExpenseTrackerAudience
```

### `docker-compose.yml` (PostgreSQL for local dev)
```yaml
version: '3.8'
services:
  postgres:
    image: postgres:16
    environment:
      POSTGRES_DB: ExpenseTracker
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data

volumes:
  postgres_data:
```

---

## Phase 6: Testing (xUnit + Moq + FluentAssertions + Testcontainers)

### Unit Tests — Services (Mock repositories)
- `Unit/Services/ExpenseServiceTests.cs` (95+ test cases)
- `Unit/Services/UserServiceTests.cs` (60+ test cases)
- Test GetAll, GetById, Create, Update, Delete with mocked repositories

### Unit Tests — Entities
- `Unit/Entities/ExpenseTests.cs` (5+ test cases)

### Integration Tests — Repositories (Real PostgreSQL)
- `Integration/Repositories/ExpenseRepositoryTests.cs` (10+ test cases)
- Uses Testcontainers.PostgreSQL, DatabaseFixture, IAsyncLifetime
- Test CRUD operations, DataReader mapping, ordering

### Integration Tests — Controllers (Mock services)
- `Integration/Controllers/ExpensesControllerTests.cs` (10+ test cases)
- `Integration/Controllers/AuthControllerTests.cs` (8+ test cases)
- Test HTTP status codes (200, 201, 204, 404, 401), response body serialization

### Fixtures
- `Fixtures/DatabaseFixture.cs` — PostgreSQL Testcontainer, connection string, async initialization
- `Fixtures/DatabaseCollection.cs` — xUnit collection fixture definition

### Helpers
- `Helpers/JwtTokenHelper.cs` — Generate valid/expired JWT tokens for testing

---

## Key Implementation Notes

1. **No ORM:** Raw Npgsql + manual SQL queries + manual DataReader mapping
2. **No AutoMapper:** Manual DTO ↔ Entity mapping in services
3. **Immutable DTOs:** Use C# `record` types
4. **Environment Variables:** Load from .env using dotenv.net
5. **Async All The Way:** All repository and service methods are async
6. **Logging:** Use ILogger<T> injected into services and middleware
7. **Error Handling:** Centralized middleware catches all exceptions
8. **Testing Strategy:**
   - Unit: Mock repositories, test service logic
   - Integration (Repo): Real database, test SQL + mapping
   - Integration (Controller): Mock services, test HTTP semantics
9. **GUID IDs:** All entities use Guid (UUID), not integers
10. **BCrypt:** Use BCrypt for password hashing (implement from scratch or via library)

---

## Verification Checklist

After implementation:
- ✅ dotnet build → 0 errors, 0 warnings
- ✅ dotnet test → All tests pass
- ✅ Swagger UI → GET /swagger shows all endpoints with Bearer token field
- ✅ Test endpoints:
  ```bash
  # Register
  curl -X POST http://localhost:5157/api/auth/register \
    -H "Content-Type: application/json" \
    -d '{"username":"testuser","password":"P@ssw0rd!","firstName":"Test","lastName":"User","email":"test@example.com"}'
  
  # Login
  curl -X POST http://localhost:5157/api/auth/login \
    -H "Content-Type: application/json" \
    -d '{"username":"testuser","password":"P@ssw0rd!"}'
  
  # Get expenses (with Bearer token from login)
  curl -X GET http://localhost:5157/api/expenses \
    -H "Authorization: Bearer <token>"
  ```

---

## Success Criteria

- Clean Architecture with clear dependency inversion
- Multi-user authentication (JWT + refresh tokens + BCrypt)
- CRUD operations for Expenses, Categories, Users, UserExpenses
- Raw ADO.NET data access (no ORM)
- Centralized exception handling
- Full test coverage (unit + integration)
- Swagger documentation with Bearer token support
- PostgreSQL as database with proper schema, constraints, indexes
