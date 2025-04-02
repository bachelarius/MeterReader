using FileHelpers;
using LanguageExt;
using LanguageExt.Common;
using MeterReaderAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace MeterReaderAPI.Accounts {
    public interface IAccountsSeedService {
        public Task<Result<Unit>> SeedAsync(CancellationToken ct);
    }

    public class AccountsSeedService(ApplicationDbContext context, ILogger<AccountsSeedService> logger) : IAccountsSeedService {
        private readonly ApplicationDbContext _context = context!;
        private ILogger<AccountsSeedService> logger = logger!;
        
        private const string _csvPath = "Resources/Test_Accounts.csv";

        public async Task<Result<Unit>> SeedAsync(CancellationToken ct) {
            logger.LogDebug("Scanning accounts csv file {filepath}", _csvPath);
            if (!File.Exists(_csvPath)) {
                logger.LogError("CSV file not found at path: {csvPath}", _csvPath);
                return new Result<Unit>(new FileNotFoundException($"CSV file not found at path: {_csvPath}", _csvPath));
            }
            
            try {
                logger.LogDebug("Parsing accounts csv file {filepath}", _csvPath);
                var engine = new FileHelperEngine<AccountCsvDTO>();
                var accounts = engine.ReadFile(_csvPath);
                logger.LogDebug("Successfully found {totalAccounts} in csv file {filepath}", accounts.Length, _csvPath);
                
                var existingIds = await _context.Accounts
                    .Select(a => a.AccountId)
                    .ToListAsync();

                var newAccounts = accounts
                    .Where(a => !existingIds.Contains(a.AccountId))
                    .Select(a => a.ToDomain())
                    .ToList();

                if (newAccounts.Any()) {
                    logger.LogDebug("Found {count} of {totalAccounts} new accounts to add", newAccounts.Count, accounts.Length);
                    await _context.Accounts.AddRangeAsync(newAccounts, ct);
                    await _context.SaveChangesAsync(ct);
                    logger.LogInformation("Added {accountsAdded} of {totalAccounts} new accounts to database", newAccounts.Count, accounts.Length);
                } else {
                    logger.LogInformation("Found {totalAccounts} accounts in CSV, all of which were already ingested", accounts.Length);
                }
                return new Result<Unit>(Unit.Default);

            } catch (Exception ex) {
                return new Result<Unit>(ex);
            }
        }
    }
}
