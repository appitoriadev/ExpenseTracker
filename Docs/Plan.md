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
    │   ├── Entities/Expense.cs
    │   └── Interfaces/IExpenseRepository.cs
    ├── ExpenseTracker.Application/     ← depends on Domain only
    │   ├── DTOs/{Create,Update,Response}ExpenseDto.cs
    │   ├── Interfaces/IExpenseService.cs
    │   └── Services/ExpenseService.cs
    ├── ExpenseTracker.Infrastructure/  ← depends on Domain only
    │   ├── Data/ConnectionProvider.cs
    │   ├── Data/Schema.sql
    │   └── Repositories/ExpenseRepository.cs
    └── ExpenseTracker.Api/             ← depends on Application + Infrastructure
        ├── Controllers/AuthController.cs
        ├── Controllers/ExpensesController.cs
        ├── Program.cs
        ├── appsettings.json
        └── appsettings.Development.json
```

**Dependency rule:** Domain ← Application ← (Infrastructure, API). Infrastructure references
Domain only; API references Application + Infrastructure for DI wiring.

---

## Phase 1 — CLI Scaffold

```bash
dotnet new sln -n ExpenseTracker

dotnet new classlib -n ExpenseTracker.Domain       -f net10.0 -o ExpenseTracker.Domain
dotnet new classlib -n ExpenseTracker.Application  -f net10.0 -o ExpenseTracker.Application
dotnet new classlib -n ExpenseTracker.Infrastructure -f net10.0 -o ExpenseTracker.Infrastructure
dotnet new webapi   -n ExpenseTracker.Api          -f net10.0 -o ExpenseTracker.Api

dotnet sln ExpenseTracker.sln add \
  ExpenseTracker.Domain/ExpenseTracker.Domain.csproj \
  ExpenseTracker.Application/ExpenseTracker.Application.csproj \
  ExpenseTracker.Infrastructure/ExpenseTracker.Infrastructure.csproj \
  ExpenseTracker.Api/ExpenseTracker.Api.csproj

# Project references
dotnet add ExpenseTracker.Application/ExpenseTracker.Application.csproj \
    reference ExpenseTracker.Domain/ExpenseTracker.Domain.csproj

dotnet add ExpenseTracker.Infrastructure/ExpenseTracker.Infrastructure.csproj \
    reference ExpenseTracker.Domain/ExpenseTracker.Domain.csproj

dotnet add ExpenseTracker.Api/ExpenseTracker.Api.csproj \
    reference ExpenseTracker.Application/ExpenseTracker.Application.csproj
dotnet add ExpenseTracker.Api/ExpenseTracker.Api.csproj \
    reference ExpenseTracker.Infrastructure/ExpenseTracker.Infrastructure.csproj

# NuGet packages
dotnet add ExpenseTracker.Infrastructure/ExpenseTracker.Infrastructure.csproj \
    package Npgsql
dotnet add ExpenseTracker.Api/ExpenseTracker.Api.csproj \
    package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add ExpenseTracker.Api/ExpenseTracker.Api.csproj \
    package Swashbuckle.AspNetCore

```

---

## Phase 2 — Source Files

### Domain

**`Entities/Expense.cs`** — plain POCO, no attributes needed  
**`Interfaces/IExpenseRepository.cs`** — GetAll, GetById, Add, Update, Delete (all async)

### Application

DTOs as C# `record` types (immutable value carriers, no AutoMapper dependency).  
**`ExpenseService.cs`** — maps entities↔DTOs, delegates to `IExpenseRepository`.

### Infrastructure

**`ConnectionProvider.cs`** — Singleton service providing NpgsqlConnection with connection string from appsettings.
**`ExpenseRepository.cs`** — Raw ADO.NET + Npgsql implementation:

- Uses `NpgsqlConnection`, `NpgsqlCommand`, `NpgsqlDataReader`
- Manual SQL queries for all CRUD operations
- Manual mapping from DataReader rows → Expense entities
- `GetAllAsync` orders by Date DESC

### API

**`AuthController.cs`** — `POST /api/auth/login` compares credentials from appsettings,
returns signed JWT + expiry.  
**`ExpensesController.cs`** — 5 verbs (GET all, GET/:id, POST, PUT/:id, DELETE/:id),
all decorated `[Authorize]`.  
**`Program.cs`** — registers ConnectionProvider (DI), JWT Bearer, CORS (React origins), Swagger with
Bearer button. No auto-migration (schema created manually or via init helper).
**`appsettings.json`** — JWT key/issuer/audience/expiry, single-user credentials,
CORS origins (`localhost:5173`, `localhost:3000`), PostgreSQL connection string.

Key middleware order in `Program.cs`:

```bash
UseCors → UseAuthentication → UseAuthorization → MapControllers
```

CORS before auth so browser pre-flight OPTIONS requests succeed before JWT inspection.

---

## Phase 3 — Database Schema

Create the `Expenses` table manually via SQL script (or execute directly from infrastructure startup):

```sql
CREATE TABLE IF NOT EXISTS expenses (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    title VARCHAR(255) NOT NULL,
    amount NUMERIC(18, 2) NOT NULL,
    category VARCHAR(100) NOT NULL,
    date TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS idx_expenses_date ON expenses(date DESC);
```

**Option 1:** Execute manually on database before first run.
**Option 2:** Add a schema initialization method in `ExpenseRepository` that runs on first connection (simple check-and-create).
**Option 3:** Provide SQL file in `Infrastructure/Data/Schema.sql` for DBA/setup documentation.

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

## Architecture Decisions

| Decision | Rationale |
| --- | --- |
| PostgreSQL (Npgsql driver) | Enterprise-ready, scalable, common in modern cloud stacks. Uses raw Npgsql driver (not EF Core) per exercise constraints |
| Raw ADO.NET in Repository | No ORM magic; manual SQL + manual mapping demonstrates clean data access layer separation and aligns with exercise requirements (no EF, Dapper, or Mediator) |
| No AutoMapper | One entity → trivial manual mapping; avoids opinionated dependency |
| `record` DTOs | Immutable value-based types; match DTO semantics exactly |
| Manual schema creation | Keeps Infrastructure layer simple; no migration framework dependency; SQL scripts provide clear schema documentation |
| Plain-text password | Acceptable for interview prototype; note BCrypt needed in prod |
| Infrastructure → Domain only | Clean Architecture inversion: API wires DI, Infrastructure stays decoupled from Application |
