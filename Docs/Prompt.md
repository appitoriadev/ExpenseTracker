# Prompts

## Initial Prompt

Act as a Senior .NET Architect. Scaffold a C# .NET Web API for an Expense Tracker application using Clean Architecture. The API will be consumed by a React frontend.

Please provide the CLI commands to create the solution and generate the core boilerplate code for the following four projects:

- ExpenseTracker.Domain: Include an Expense entity (Id, Title, Amount, Category, Date) and an IExpenseRepository interface. This layer must have no external dependencies

- ExpenseTracker.Application: Include a service for basic CRUD operations on Expenses and the necessary DTOs

- ExpenseTracker.Infrastructure: Implement the repository interface using Entity Framework Core and provide the Database Context

- ExpenseTracker.Api: Include an ExpensesController with standard HTTP verbs, wire up Dependency Injection for the layers, and configure basic JWT Authentication for a single user.

## Usefull Commands

Docker compose launch with .env local file.

```bash
docker compose -f docker-compose.yml -f docker-compose.override.yml --env-file ./backend/ExpenseTracker.Api/.env.local up -d
```