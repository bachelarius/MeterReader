using MeterReaderAPI.MeterReadings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using MeterReaderAPI.Data;
using Moq;

namespace MeterReaderAPITests.MeterReadings;

public class MeterReadingUploadsHandlerTests : IDisposable
{
    private readonly Mock<IMeterReadingExtractorService> _mockExtractor;
    private readonly Mock<IMeterReadingValidator> _mockValidator;
    private readonly ApplicationDbContext _context;

    public MeterReadingUploadsHandlerTests()
    {
        _mockExtractor = new Mock<IMeterReadingExtractorService>();
        _mockValidator = new Mock<IMeterReadingValidator>();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
    }

    [Fact]
    public async Task Handle_WithValidCsv_SavesReadingsToDatabase()
    {
        // Arrange
        var csvContent = "AccountId,MeterReadingDateTime,MeterReadValue\n2344,22/04/2019 09:24,1002";
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns("test.csv");
        mockFile.Setup(f => f.Length).Returns(csvContent.Length);
        mockFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(csvContent)));

        var expectedDto = new MeterReadingCsvDTO 
        { 
            AccountId = 2344, 
            MeterReadingDateTime = "22/04/2019 09:24", 
            MeterReadValue = 1002 
        };
        var expectedReading = new MeterReading(
            Guid.NewGuid(),
            expectedDto.AccountId,
            DateTimeOffset.Parse(expectedDto.MeterReadingDateTime),
            expectedDto.MeterReadValue
        );

        _mockExtractor.Setup(x => x.ExtractMeterReadings(It.IsAny<string>()))
            .Returns([expectedDto]);
        _mockValidator.Setup(x => x.ValidateReading(It.IsAny<MeterReadingCsvDTO>()))
            .Returns([expectedReading]);

        // Act
        var result = await MeterReadingUploadsHandler.Handle(
            mockFile.Object, 
            _mockExtractor.Object, 
            _mockValidator.Object,
            _context);

        // Assert
        Assert.IsType<Ok<object>>(result.Result);
        var savedReading = await _context.Set<MeterReading>().FirstOrDefaultAsync();
        Assert.NotNull(savedReading);
        Assert.Equal(expectedReading.AccountId, savedReading.AccountId);
        Assert.Equal(expectedReading.MeterReadingValue, savedReading.MeterReadingValue);
        Assert.Equal(expectedReading.MeterReadingDateTime, savedReading.MeterReadingDateTime);
    }

    [Fact]
    public async Task Handle_WithNoFile_ReturnsBadRequest() {
        // Act
        var result = await MeterReadingUploadsHandler.Handle(null!, _mockExtractor.Object, _mockValidator.Object, _context);

        // Assert
        Assert.True(result.Result is BadRequest<string>);
    }

    [Fact]
    public async Task Handle_WithNonCsvFile_ReturnsBadRequest() {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns("test.txt");

        // Act
        var result = await MeterReadingUploadsHandler.Handle(mockFile.Object, _mockExtractor.Object, _mockValidator.Object, _context);

        // Assert
        Assert.True(result.Result is BadRequest<string>);
    }

    public void Dispose() {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
