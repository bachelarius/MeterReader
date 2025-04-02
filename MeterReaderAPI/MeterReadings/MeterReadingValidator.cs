using MeterReaderAPI.Data;

namespace MeterReaderAPI.MeterReadings {
    public interface IMeterReadingValidator {
        IEnumerable<MeterReading> ValidateReading(MeterReadingCsvDTO meterReading);
    }
    public class MeterReadingValidator(ApplicationDbContext context) : IMeterReadingValidator {
        private readonly ApplicationDbContext _context = context;

        public IEnumerable<MeterReading> ValidateReading(MeterReadingCsvDTO meterReading) {
            // Validate Account ID exists
            var accountExists = _context.Accounts.Any(a => a.AccountId == meterReading.AccountId);
            if (!accountExists) {
                yield break;
            }

            // Validate meter reading format (NNNNN - 5 digits or less)
            if (meterReading.MeterReadValue < 0 || meterReading.MeterReadValue > 99999) {
                yield break;
            }

            // Check that date is valid and in the expected format (dd/MM/yyyy HH:mm)
            if (!TryParseDateTime(meterReading.MeterReadingDateTime, out var readingDateTime)) {
                yield break;
            }

            // Check for duplicate entry
            var isDuplicate = _context.Set<MeterReading>().Any(r =>
                r.AccountId == meterReading.AccountId &&
                r.MeterReadingDateTime == readingDateTime &&
                r.MeterReadingValue == meterReading.MeterReadValue);

            if (isDuplicate) {
                yield break;
            }

            // If all validations pass, return the domain object
            yield return new MeterReading(
                Guid.NewGuid(),
                meterReading.AccountId,
                readingDateTime,
                meterReading.MeterReadValue
            );
        }

        private static bool TryParseDateTime(string dateTimeString, out DateTimeOffset result)
        {
            result = DateTimeOffset.MinValue;

            // Check if the string is empty
            if (string.IsNullOrWhiteSpace(dateTimeString))
                return false;

            // Check if the string contains time component (must have a colon)
            // This will reject formats like "22-04-2019" (missing time), "2024-03-20" (missing time)
            if (!dateTimeString.Contains(':'))
                return false;

            // Try to parse with the expected formats
            // Accept both "dd/MM/yyyy HH:mm" and "yyyy-MM-dd HH:mm:ss" formats
            var formats = new[] { "dd/MM/yyyy HH:mm", "yyyy-MM-dd HH:mm:ss" };
            return DateTimeOffset.TryParseExact(dateTimeString, formats, null, System.Globalization.DateTimeStyles.None, out result);
        }
    }
}
