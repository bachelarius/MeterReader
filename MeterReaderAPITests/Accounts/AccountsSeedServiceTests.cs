using MeterReaderAPI.Accounts;
using MeterReaderAPI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace MeterReaderAPITests.Accounts;

public class AccountsSeedServiceTests : IDisposable {
    private readonly ApplicationDbContext _context;
    private readonly AccountsSeedService _sut;

    public AccountsSeedServiceTests() {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _sut = new AccountsSeedService(_context, Mock.Of<ILogger<AccountsSeedService>>());
    }

    [Fact]
    public async Task SeedAsync_WithNewAccounts_ShouldAddToDatabase() {
        // Arrange
        var accounts = new List<Account>
        {
            new() { AccountId = 1, FirstName = "John", LastName = "Doe" },
            new() { AccountId = 2, FirstName = "Jane", LastName = "Smith" }
        };

        // Act
        await _sut.SeedAsync(accounts, CancellationToken.None);

        // Assert
        var savedAccounts = await _context.Accounts.ToListAsync();
        Assert.Equal(2, savedAccounts.Count);

        var john = savedAccounts.First(a => a.AccountId == 1);
        Assert.Equal("John", john.FirstName);
        Assert.Equal("Doe", john.LastName);

        var jane = savedAccounts.First(a => a.AccountId == 2);
        Assert.Equal("Jane", jane.FirstName);
        Assert.Equal("Smith", jane.LastName);
    }

    [Fact]
    public async Task SeedAsync_WithExistingAccounts_ShouldNotDuplicate() {
        // Arrange
        var existingAccount = new Account { AccountId = 1, FirstName = "John", LastName = "Doe" };
        await _context.Accounts.AddAsync(existingAccount);
        await _context.SaveChangesAsync();

        var accounts = new List<Account>
        {
            new() { AccountId = 1, FirstName = "John", LastName = "Doe" }, // Existing
            new() { AccountId = 2, FirstName = "Jane", LastName = "Smith" }  // New
        };

        // Act
        await _sut.SeedAsync(accounts, CancellationToken.None);

        // Assert
        var savedAccounts = await _context.Accounts.ToListAsync();
        Assert.Equal(2, savedAccounts.Count);

        var john = savedAccounts.First(a => a.AccountId == 1);
        Assert.Equal("John", john.FirstName);
        Assert.Equal("Doe", john.LastName);

        var jane = savedAccounts.First(a => a.AccountId == 2);
        Assert.Equal("Jane", jane.FirstName);
        Assert.Equal("Smith", jane.LastName);
    }

    [Fact]
    public async Task SeedAsync_WithEmptyList_ShouldNotModifyDatabase() {
        // Arrange
        var accounts = new List<Account>();

        // Act
        await _sut.SeedAsync(accounts, CancellationToken.None);

        // Assert
        var savedAccounts = await _context.Accounts.ToListAsync();
        Assert.Empty(savedAccounts);
    }

    [Fact]
    public async Task SeedAsync_WithAllExistingAccounts_ShouldNotModifyDatabase() {
        // Arrange
        var existingAccount = new Account { AccountId = 1, FirstName = "John", LastName = "Doe" };
        await _context.Accounts.AddAsync(existingAccount);
        await _context.SaveChangesAsync();

        var accounts = new List<Account>
        {
            new() { AccountId = 1, FirstName = "John", LastName = "Doe" }
        };

        // Act
        await _sut.SeedAsync(accounts, CancellationToken.None);

        // Assert
        var savedAccounts = await _context.Accounts.ToListAsync();
        Assert.Single(savedAccounts);

        var john = savedAccounts.First();
        Assert.Equal(1, john.AccountId);
        Assert.Equal("John", john.FirstName);
        Assert.Equal("Doe", john.LastName);
    }

    public void Dispose() {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
