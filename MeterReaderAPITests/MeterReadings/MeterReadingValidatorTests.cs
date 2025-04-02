using MeterReaderAPI.Accounts;
using MeterReaderAPI.Data;
using MeterReaderAPI.MeterReadings;
using Microsoft.EntityFrameworkCore;

namespace MeterReaderAPITests.MeterReadings;

public class MeterReadingValidatorTests : IDisposable {
    private readonly ApplicationDbContext _context;
    private readonly MeterReadingValidator _validator;

    public MeterReadingValidatorTests() {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _validator = new MeterReadingValidator(_context);

        // Seed test accounts
        _context.Accounts.AddRange(
        [
            new Account { AccountId = 1234 },
            new Account { AccountId = 5678 }
        ]);
        _context.SaveChanges();
    }

    [Fact]
    public void ExtractMeterReadings_WithValidReading_ReturnsReading() {
        // Arrange
        var dto = new MeterReadingCsvDTO {
            AccountId = 1234,
            MeterReadingDateTime = "2024-03-20 10:00:00",
            MeterReadValue = 12345
        };

        // Act
        var result = _validator.ValidateReading(dto).ToList();

        // Assert
        Assert.Single(result);
        var reading = result[0];
        Assert.Equal(dto.AccountId, reading.AccountId);
        Assert.Equal(dto.MeterReadValue, reading.MeterReadingValue);
        Assert.Equal(DateTimeOffset.Parse(dto.MeterReadingDateTime), reading.MeterReadingDateTime);
    }

    [Fact]
    public void ExtractMeterReadings_WithNonExistentAccount_ReturnsEmpty() {
        // Arrange
        var dto = new MeterReadingCsvDTO {
            AccountId = 9999, // Non-existent account
            MeterReadingDateTime = "2024-03-20 10:00:00",
            MeterReadValue = 12345
        };

        // Act
        var result = _validator.ValidateReading(dto).ToList();

        // Assert
        Assert.Empty(result);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(100000)]
    public void ExtractMeterReadings_WithInvalidReadingValue_ReturnsEmpty(int invalidValue) {
        // Arrange
        var dto = new MeterReadingCsvDTO {
            AccountId = 1234,
            MeterReadingDateTime = "2024-03-20 10:00:00",
            MeterReadValue = invalidValue
        };

        // Act
        var result = _validator.ValidateReading(dto).ToList();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void ExtractMeterReadings_WithDuplicateReading_ReturnsEmpty() {
        // Arrange
        var dto = new MeterReadingCsvDTO {
            AccountId = 1234,
            MeterReadingDateTime = "2024-03-20 10:00:00",
            MeterReadValue = 12345
        };

        // Add initial reading
        _context.Set<MeterReading>().Add(new MeterReading(
            Guid.NewGuid(),
            dto.AccountId,
            DateTimeOffset.Parse(dto.MeterReadingDateTime),
            dto.MeterReadValue
        ));
        _context.SaveChanges();

        // Act
        var result = _validator.ValidateReading(dto).ToList();

        // Assert
        Assert.Empty(result);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(99999)]
    public void ExtractMeterReadings_WithBoundaryValues_ReturnsReading(int value) {
        // Arrange
        var dto = new MeterReadingCsvDTO {
            AccountId = 1234,
            MeterReadingDateTime = "2024-03-20 10:00:00",
            MeterReadValue = value
        };

        // Act
        var result = _validator.ValidateReading(dto).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal(value, result[0].MeterReadingValue);
    }

    [Theory]
    [InlineData("invalid-date")]
    [InlineData("2024/13/45")]
    [InlineData("22-04-2019")] // Missing time component
    [InlineData("2024-03-20")] // Missing time component
    [InlineData("09:24 22/04/2019")] // Wrong order
    [InlineData("")] // Empty string
    public void ExtractMeterReadings_WithInvalidDateFormat_ReturnsEmpty(string invalidDate)
    {
        // Arrange
        var dto = new MeterReadingCsvDTO
        {
            AccountId = 1234,
            MeterReadingDateTime = invalidDate,
            MeterReadValue = 12345
        };

        // Act
        var result = _validator.ValidateReading(dto).ToList();

        // Assert
        Assert.Empty(result);
    }

    public void Dispose() {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}

