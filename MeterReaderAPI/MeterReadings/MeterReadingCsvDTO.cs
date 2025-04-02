using FileHelpers;

namespace MeterReaderAPI.MeterReadings {
    [DelimitedRecord(",")]
    [IgnoreFirst(1)]
    [IgnoreEmptyLines]
    public class MeterReadingCsvDTO {
        public int AccountId { get; set; }
        
        [FieldConverter(ConverterKind.Date, "dd-MM-yyyy")]
        public DateTimeOffset MeterReadingDateTime { get; set; }
        
        public int MeterReadingValue { get; set; }
        
        public MeterReading ToDomain(Guid? id = null) => new(
            Id: id ?? Guid.NewGuid(),
            AccountId: AccountId,
            MeterReadingDateTime: MeterReadingDateTime,
            MeterReadingValue: MeterReadingValue);
    }
}
