using FileHelpers;

namespace MeterReaderAPI.Accounts {

    public interface IAccountsExtractorService {
        public IEnumerable<AccountCsvDTO> ExtractAccounts();
    }

    public class AccountExtractorService(ILogger<AccountExtractorService> logger) : IAccountsExtractorService {
        private readonly ILogger<AccountExtractorService> logger = logger!;
        const string _csvPath = "Resources/Test_Accounts.csv";

        public IEnumerable<AccountCsvDTO> ExtractAccounts() {
            logger.LogDebug("Scanning accounts csv file {filepath}", _csvPath);
            if (!File.Exists(_csvPath)) {
                logger.LogError("CSV file not found at path: {csvPath}", _csvPath);
                throw new FileNotFoundException($"CSV file not found at path: {_csvPath}", _csvPath);
            }

            logger.LogDebug("Parsing accounts csv file {filepath}", _csvPath);
            var engine = new FileHelperEngine<AccountCsvDTO>();
            var accounts = engine.ReadFile(_csvPath);

            logger.LogDebug("Successfully found {totalAccounts} in csv file {filepath}", accounts.Length, _csvPath);
            return accounts;
        }
    }
}
