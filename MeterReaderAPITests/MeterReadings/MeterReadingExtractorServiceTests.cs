using MeterReaderAPI.MeterReadings;
using Microsoft.Extensions.Logging;
using Moq;

namespace MeterReaderAPITests.MeterReadings;

public class MeterReadingExtractorServiceTests {
    [Fact]
    public void ExtractMeterReadings_WithValidCsv_ShouldReturnMeterReadings() {
        // Arrange
        var sut = new MeterReadingExtractorService(Mock.Of<ILogger<MeterReadingExtractorService>>());
        var csvContent = File.ReadAllText("Resources/Meter_Reading.csv");

        // Act
        var meterReadings = sut.ExtractMeterReadings(csvContent).ToList();

        // Assert
        var lines = csvContent.Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries)
            .Skip(1).ToList();
        Assert.NotEmpty(meterReadings);
        for (var i = 0; i < meterReadings.Count; i++) {
            var line = lines[i];
            var reading = meterReadings[i];
            Assert.Equal(line, reading.ToString());
        }
        Assert.Equal(lines.Count, meterReadings.Count);
    }

    [Fact]
    public void ExtractMeterReadings_WithValidCsv_ShouldParseCorrectly() {
        // Arrange
        var sut = new MeterReadingExtractorService(Mock.Of<ILogger<MeterReadingExtractorService>>());
        var csvContent = File.ReadAllText("Resources/Meter_Reading.csv");

        // Act
        var meterReadings = sut.ExtractMeterReadings(csvContent).ToList();

        // Assert
        // Check the first entry
        var firstReading = meterReadings.First();
        Assert.Equal(2344, firstReading.AccountId);
        Assert.Equal(1002, firstReading.MeterReadValue);

        // Check a specific entry (e.g., the one for account 8766)
        var specificReading = meterReadings.First(r => r.AccountId == 8766);
        Assert.Equal(3440, specificReading.MeterReadValue);
    }

    [Fact]
    public void ExtractMeterReadings_WithEmptyCsv_ShouldReturnEmptyCollection() {
        // Arrange
        var sut = new MeterReadingExtractorService(Mock.Of<ILogger<MeterReadingExtractorService>>());
        var csvContent = "AccountId,MeterReadingDateTime,MeterReadValue,\n";

        // Act
        var meterReadings = sut.ExtractMeterReadings(csvContent).ToList();

        // Assert
        Assert.Empty(meterReadings);
    }

    [Fact]
    public void ExtractMeterReadings_WithNegativeValues_ShouldParseCorrectly() {
        // Arrange
        var sut = new MeterReadingExtractorService(Mock.Of<ILogger<MeterReadingExtractorService>>());
        var csvContent = File.ReadAllText("Resources/Meter_Reading.csv");

        // Act
        var meterReadings = sut.ExtractMeterReadings(csvContent).ToList();

        // Assert
        // Find the entry with negative value (account 6776 with value -6575)
        var negativeReading = meterReadings.FirstOrDefault(r => r.MeterReadValue < 0);
        Assert.NotNull(negativeReading);
        Assert.Equal(6776, negativeReading.AccountId);
        Assert.Equal(-6575, negativeReading.MeterReadValue);
    }

    [Fact]
    public void ExtractMeterReadings_WithDuplicateAccountIds_ShouldReturnAllEntries() {
        // Arrange
        var sut = new MeterReadingExtractorService(Mock.Of<ILogger<MeterReadingExtractorService>>());
        var csvContent = File.ReadAllText("Resources/Meter_Reading.csv");

        // Act
        var meterReadings = sut.ExtractMeterReadings(csvContent).ToList();

        // Assert
        // Check for duplicate account IDs (e.g., account 2344 appears multiple times)
        var account2344Readings = meterReadings.Where(r => r.AccountId == 2344).ToList();
        Assert.True(account2344Readings.Count > 1);
    }

    [Fact]
    public void ExtractMeterReadings_ToDomain_ShouldCreateValidDomainObjects() {
        // Arrange
        var sut = new MeterReadingExtractorService(Mock.Of<ILogger<MeterReadingExtractorService>>());
        var csvContent = File.ReadAllText("Resources/Meter_Reading.csv");

        // Act
        var meterReadingDtos = sut.ExtractMeterReadings(csvContent).ToList();
        var domainObjects = meterReadingDtos.Select(dto => dto.ToDomain()).ToList();

        // Assert
        Assert.Equal(meterReadingDtos.Count, domainObjects.Count);

        // Check that the domain objects have the correct properties
        foreach (var (dto, domain) in meterReadingDtos.Zip(domainObjects)) {
            Assert.Equal(dto.AccountId, domain.AccountId);
            Assert.Equal(DateTimeOffset.Parse(dto.MeterReadingDateTime), domain.MeterReadingDateTime);
            Assert.Equal(dto.MeterReadValue, domain.MeterReadingValue);
            Assert.NotEqual(Guid.Empty, domain.Id);
        }
    }
}
