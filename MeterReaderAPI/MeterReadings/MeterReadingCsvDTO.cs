using FileHelpers;

namespace MeterReaderAPI.MeterReadings {

    [DelimitedRecord(",")]
    [IgnoreFirst(1)]
    [IgnoreEmptyLines]
    public class MeterReadingCsvDTO {
        public int AccountId { get; set; }

        public string MeterReadingDateTime { get; set; } = "";

        [FieldConverter(ConverterKind.Int32)]
        public int MeterReadValue { get; set; }

        [FieldOptional]
        public string[] Ignored { get; set; } = [];

        public override string ToString() {
            return $"{AccountId},{MeterReadingDateTime},{MeterReadValue},{string.Join(',', Ignored)}";
        }

        public MeterReading ToDomain(Guid? id = null) => new(
            Id: id ?? Guid.NewGuid(),
            AccountId: AccountId,
            MeterReadingDateTime: DateTimeOffset.Parse(MeterReadingDateTime),
            MeterReadingValue: MeterReadValue);
    }
}
