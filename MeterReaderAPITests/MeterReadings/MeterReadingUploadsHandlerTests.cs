using MeterReaderAPI.MeterReadings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;

namespace MeterReaderAPITests.MeterReadings;

public class MeterReadingUploadsHandlerTests {
    private readonly Mock<IMeterReadingExtractorService> _mockExtractor;
    private readonly Mock<IMeterReadingValidator> _mockValidator;

    public MeterReadingUploadsHandlerTests() {
        _mockExtractor = new Mock<IMeterReadingExtractorService>();
        _mockValidator = new Mock<IMeterReadingValidator>();
    }

    [Fact]
    public async Task Handle_WithValidCsv_ReturnsOk() {
        // Arrange
        var csvContent = "AccountId,MeterReadingDateTime,MeterReadValue\n2344,22/04/2019 09:24,1002";
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns("test.csv");
        mockFile.Setup(f => f.Length).Returns(csvContent.Length);
        mockFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(csvContent)));

        var expectedReadings = new List<MeterReadingCsvDTO>
        {
            new() { AccountId = 2344, MeterReadingDateTime = "22/04/2019 09:24", MeterReadValue = 1002 }
        };
        _mockExtractor.Setup(x => x.ExtractMeterReadings(It.IsAny<string>())).Returns(expectedReadings);

        // Act
        var result = await MeterReadingUploadsHandler.Handle(mockFile.Object, _mockExtractor.Object, _mockValidator.Object);

        // Assert
        Assert.IsType<Results<Ok<object>, BadRequest<string>, StatusCodeHttpResult>>(result);
        Assert.True(result.Result is Ok<object>);
    }

    [Fact]
    public async Task Handle_WithNoFile_ReturnsBadRequest() {
        // Act
        var result = await MeterReadingUploadsHandler.Handle(null!, _mockExtractor.Object, _mockValidator.Object);

        // Assert
        Assert.True(result.Result is BadRequest<string>);
    }

    [Fact]
    public async Task Handle_WithNonCsvFile_ReturnsBadRequest() {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns("test.txt");

        // Act
        var result = await MeterReadingUploadsHandler.Handle(mockFile.Object, _mockExtractor.Object, _mockValidator.Object);

        // Assert
        Assert.True(result.Result is BadRequest<string>);
    }
}
