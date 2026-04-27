using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Infrastructure.Data;
using ExpenseTracker.Infrastructure.Repositories;
using ExpenseTracker.Tests.Fixtures;
using FluentAssertions;
using Npgsql;

namespace ExpenseTracker.Tests.Integration.Repositories;

[Collection("Database collection")]
public class UserRepositoryTests : IAsyncLifetime
{
    private readonly DatabaseFixture _fixture;
    private UserRepository _repository = null!;
    private NpgsqlConnection _connection = null!;

    public UserRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        var connectionProvider = new ConnectionProvider(_fixture.ConnectionString);
        _repository = new UserRepository(connectionProvider);

        _connection = new NpgsqlConnection(_fixture.ConnectionString);
        await _connection.OpenAsync();
        await ClearUsers();
    }

    public async Task DisposeAsync()
    {
        if (_connection?.State == System.Data.ConnectionState.Open)
        {
            await _connection.CloseAsync();
        }
        _connection?.Dispose();
    }

    private async Task ClearUsers()
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = "DELETE FROM dbo.user_expenses; DELETE FROM dbo.users";
        await cmd.ExecuteNonQueryAsync();
    }

    private static User BuildUser(string suffix = "") => new()
    {
        Username     = $"testuser{suffix}",
        PasswordHash = "hashedpassword",
        FirstName    = "Test",
        LastName     = "User",
        Email        = $"test{suffix}@example.com",
        CreatedAt    = DateTime.UtcNow
    };

    #region AddAsync Tests

    [Fact]
    public async Task AddAsync_WithValidUser_InsertsAndReturnsWithId()
    {
        var user = BuildUser("_add");

        var result = await _repository.AddAsync(user);

        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Username.Should().Be("testuser_add");
    }

    [Fact]
    public async Task AddAsync_WithMultipleUsers_AllAreInserted()
    {
        var users = new[]
        {
            BuildUser("_m1"),
            BuildUser("_m2"),
            BuildUser("_m3")
        };

        var results = new List<User>();
        foreach (var u in users)
            results.Add(await _repository.AddAsync(u));

        results.Should().HaveCount(3);
        results.Should().AllSatisfy(r => r.Id.Should().BeGreaterThan(0));
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_WithMultipleUsers_ReturnsAll()
    {
        await _repository.AddAsync(BuildUser("_ga1"));
        await _repository.AddAsync(BuildUser("_ga2"));

        var result = await _repository.GetAllAsync();

        result.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task GetAllAsync_WithEmptyDatabase_ReturnsEmpty()
    {
        var result = await _repository.GetAllAsync();

        result.Should().BeEmpty();
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsUser()
    {
        var created = await _repository.AddAsync(BuildUser("_gid"));

        var result = await _repository.GetByIdAsync(created.Id);

        result.Should().NotBeNull();
        result!.Username.Should().Be("testuser_gid");
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ReturnsNull()
    {
        var result = await _repository.GetByIdAsync(0);

        result.Should().BeNull();
    }

    #endregion

    #region GetByUsernameAsync Tests

    [Fact]
    public async Task GetByUsernameAsync_WithValidUsername_ReturnsUser()
    {
        await _repository.AddAsync(BuildUser("_gun"));

        var result = await _repository.GetByUsernameAsync("testuser_gun");

        result.Should().NotBeNull();
        result!.Email.Should().Be("test_gun@example.com");
    }

    [Fact]
    public async Task GetByUsernameAsync_WithInvalidUsername_ReturnsNull()
    {
        var result = await _repository.GetByUsernameAsync("nonexistent_xyz");

        result.Should().BeNull();
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WithValidUser_UpdatesAllProperties()
    {
        var created = await _repository.AddAsync(BuildUser("_upd"));

        created.FirstName = "Updated";
        created.LastName  = "Name";

        var result = await _repository.UpdateAsync(created);

        result.Should().NotBeNull();
        result!.FirstName.Should().Be("Updated");
        result.LastName.Should().Be("Name");

        var retrieved = await _repository.GetByIdAsync(created.Id);
        retrieved!.FirstName.Should().Be("Updated");
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistentId_ReturnsNull()
    {
        var ghost = new User
        {
            Id           = 0,
            Username     = "ghost",
            PasswordHash = "hash",
            FirstName    = "Ghost",
            LastName     = "User"
        };

        var result = await _repository.UpdateAsync(ghost);

        result.Should().BeNull();
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WithValidId_DeletesUser()
    {
        var created = await _repository.AddAsync(BuildUser("_del"));

        var deleteResult = await _repository.DeleteAsync(created.Id);

        deleteResult.Should().BeTrue();

        var retrieved = await _repository.GetByIdAsync(created.Id);
        retrieved.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WithInvalidId_ReturnsFalse()
    {
        var result = await _repository.DeleteAsync(999999);

        result.Should().BeFalse();
    }

    #endregion
}
