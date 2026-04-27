using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Infrastructure.Data;
using ExpenseTracker.Infrastructure.Repositories;
using ExpenseTracker.Tests.Fixtures;
using FluentAssertions;
using Npgsql;

namespace ExpenseTracker.Tests.Integration.Repositories;

[Collection("Database collection")]
public class ExpenseRepositoryTests : IAsyncLifetime
{
    private readonly DatabaseFixture _fixture;
    private ExpenseRepository _repository = null!;
    private static Guid guid;
    private NpgsqlConnection _connection = null!;

    public ExpenseRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        var connectionProvider = new ConnectionProvider(_fixture.ConnectionString);
        _repository = new ExpenseRepository(connectionProvider);
        guid = Guid.NewGuid();

        _connection = new NpgsqlConnection(_fixture.ConnectionString);
        await _connection.OpenAsync();
        await CreateCategories();
    }

    private async Task CreateCategories()
    {
        var categories = new[] { "Testing", "Cat1", "Cat2", "Cat3", "Cat", "Test", "OrigCat", "UpdatedCat" };

        foreach (var cat in categories)
        {
            try
            {
                using var cmd = _connection.CreateCommand();
                cmd.CommandText = "INSERT INTO categories (category_name) VALUES (@name) ON CONFLICT DO NOTHING";
                cmd.Parameters.AddWithValue("@name", cat);
                await cmd.ExecuteNonQueryAsync();
            }
            catch
            {
                // Category might already exist
            }
        }
    }

    public async Task DisposeAsync()
    {
        if (_connection?.State == System.Data.ConnectionState.Open)
        {
            await _connection.CloseAsync();
        }
        _connection?.Dispose();
    }

    #region AddAsync Tests

    [Fact]
    public async Task AddAsync_WithValidExpense_InsertsAndReturnsWithId()
    {
        var expense = new Expense
        {
            Title = "Test Expense",
            Amount = 99.99m,
            CategoryName = "Testing",
            Date = DateTime.Now
        };

        var result = await _repository.AddAsync(expense);

        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.Title.Should().Be("Test Expense");
        result.Amount.Should().Be(99.99m);
    }

    [Fact]
    public async Task AddAsync_WithMultipleExpenses_AllAreInserted()
    {
        var expenses = new[]
        {
            new Expense { Title = "Exp 1", Amount = 10m, CategoryName = "Cat1", Date = DateTime.Now },
            new Expense { Title = "Exp 2", Amount = 20m, CategoryName = "Cat2", Date = DateTime.Now },
            new Expense { Title = "Exp 3", Amount = 30m, CategoryName = "Cat3", Date = DateTime.Now }
        };

        var results = new List<Expense>();
        foreach (var exp in expenses)
        {
            results.Add(await _repository.AddAsync(exp));
        }

        results.Should().HaveCount(3);
        results.Should().AllSatisfy(r => r.Id.Should().NotBeEmpty());
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_WithMultipleExpenses_ReturnsAllOrderedByDateDesc()
    {
        await ClearExpenses();

        var now = DateTime.Now;
        var expenses = new[]
        {
            new Expense { Title = "Old", Amount = 10m, CategoryName = "Cat", Date = now.AddDays(-2) },
            new Expense { Title = "New", Amount = 20m, CategoryName = "Cat", Date = now },
            new Expense { Title = "Middle", Amount = 15m, CategoryName = "Cat", Date = now.AddDays(-1) }
        };

        foreach (var exp in expenses)
        {
            await _repository.AddAsync(exp);
        }

        var result = (await _repository.GetAllAsync()).ToList();

        result.Should().HaveCount(3);
        result.First().Title.Should().Be("New");
        result.Should().BeInDescendingOrder(e => e.Date);
    }

    [Fact]
    public async Task GetAllAsync_WithEmptyDatabase_ReturnsEmpty()
    {
        await ClearExpenses();
        var result = await _repository.GetAllAsync();

        result.Should().BeEmpty();
    }

    private async Task ClearExpenses()
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = "DELETE FROM expenses";
        await cmd.ExecuteNonQueryAsync();
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsExpense()
    {
        var expense = new Expense
        {
            Title = "Find Me",
            Amount = 55m,
            CategoryName = "Test",
            Date = DateTime.Now
        };

        var created = await _repository.AddAsync(expense);
        var result = await _repository.GetByIdAsync(created.Id);

        result.Should().NotBeNull();
        result!.Title.Should().Be("Find Me");
        result.Amount.Should().Be(55m);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ReturnsNull()
    {
        var result = await _repository.GetByIdAsync(new Guid());

        result.Should().BeNull();
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WithValidExpense_UpdatesAllProperties()
    {
        var expense = new Expense
        {
            Title = "Original",
            Amount = 10m,
            CategoryName = "OrigCat",
            Date = DateTime.Now
        };

        var created = await _repository.AddAsync(expense);

        created.Title = "Updated";
        created.Amount = 25m;
        created.CategoryName = "UpdatedCat";
        created.Date = DateTime.Now.AddDays(1);

        var result = await _repository.UpdateAsync(created);

        result.Should().NotBeNull();
        result!.Title.Should().Be("Updated");
        result.Amount.Should().Be(25m);
        result.CategoryName.Should().Be("UpdatedCat");

        var retrieved = await _repository.GetByIdAsync(created.Id);
        retrieved!.Title.Should().Be("Updated");
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistentId_ReturnsNull()
    {
        var expense = new Expense
        {
            Id = new Guid(),
            Title = "Ghost",
            Amount = 10m,
            CategoryName = "Cat",
            Date = DateTime.Now
        };

        var result = await _repository.UpdateAsync(expense);

        result.Should().BeNull();
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WithValidId_DeletesExpense()
    {
        var expense = new Expense
        {
            Title = "Delete Me",
            Amount = 10m,
            CategoryName = "Test",
            Date = DateTime.Now
        };

        var created = await _repository.AddAsync(expense);
        var deleteResult = await _repository.DeleteAsync(created.Id);

        deleteResult.Should().BeTrue();

        var retrieved = await _repository.GetByIdAsync(created.Id);
        retrieved.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WithInvalidId_ReturnsFalse()
    {
        var result = await _repository.DeleteAsync(Guid.NewGuid());

        result.Should().BeFalse();
    }

    #endregion
}
