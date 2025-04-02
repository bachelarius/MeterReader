namespace MeterReaderAPI.MeterReadings {
    public record MeterReading(Guid Id, int AccountId, DateTimeOffset MeterReadingDateTime, int MeterReadingValue) { }
}
