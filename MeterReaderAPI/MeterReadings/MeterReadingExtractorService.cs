using FileHelpers;
using MeterReaderAPI.Accounts;
using Microsoft.Extensions.Logging;

namespace MeterReaderAPI.MeterReadings {
    
    public interface IMeterReadingExtractorService {
        IEnumerable<MeterReadingCsvDTO> ExtractMeterReadings(string csvFileContent);
    }

    public class MeterReadingExtractorService(ILogger<MeterReadingExtractorService> logger) : IMeterReadingExtractorService {
        private readonly ILogger<MeterReadingExtractorService> logger = logger;

        public IEnumerable<MeterReadingCsvDTO> ExtractMeterReadings(string csvFileContent) {
            logger.LogDebug("Parsing stream as csv file");
            var engine = new FileHelperEngine<MeterReadingCsvDTO>();
            var meterReadings = engine.ReadString(csvFileContent);

            logger.LogDebug("Successfully found {totalMeterReadings} in csv file", meterReadings.Length);
            return meterReadings;
        }
    }
}
