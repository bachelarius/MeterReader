namespace MeterReaderAPI.MeterReadings;

public class MeterReadingUploadRequest
{
    public IFormFile File { get; set; } = null!;
}