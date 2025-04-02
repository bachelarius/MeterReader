namespace MeterReaderAPI.MeterReadings {
    
    public interface IMeterReadingExtractorService {
        IEnumerable<MeterReadingCsvDTO> ExtractMeterReadings();
    }


    public class MeterReadingExtractorService : IMeterReadingExtractorService {
        public IEnumerable<MeterReadingCsvDTO> ExtractMeterReadings() {
            throw new NotImplementedException();
        }
    }
}
