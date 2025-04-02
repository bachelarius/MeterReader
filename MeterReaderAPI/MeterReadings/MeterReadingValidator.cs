using MeterReaderAPI.Data;

namespace MeterReaderAPI.MeterReadings {
    public interface IMeterReadingValidator {
        IEnumerable<MeterReading> ExtractMeterReadings(MeterReadingCsvDTO meterReading);
    }
    public class MeterReadingValidator(ApplicationDbContext context) : IMeterReadingValidator {
        private readonly ApplicationDbContext _context = context;

        public IEnumerable<MeterReading> ExtractMeterReadings(MeterReadingCsvDTO meterReading) {
            // Validate Account ID exists
            var accountExists = _context.Accounts.Any(a => a.AccountId == meterReading.AccountId);
            if (!accountExists) {
                yield break;
            }

            // Validate meter reading format (NNNNN - 5 digits or less)
            if (meterReading.MeterReadValue < 0 || meterReading.MeterReadValue > 99999) {
                yield break;
            }

            // Check for duplicate entry
            var readingDateTime = DateTimeOffset.Parse(meterReading.MeterReadingDateTime);
            var isDuplicate = _context.Set<MeterReading>().Any(r => 
                r.AccountId == meterReading.AccountId && 
                r.MeterReadingDateTime == readingDateTime && 
                r.MeterReadingValue == meterReading.MeterReadValue);

            if (isDuplicate) {
                yield break;
            }

            // If all validations pass, return the domain object
            yield return meterReading.ToDomain();
        }
    }
}
