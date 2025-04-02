using MeterReaderAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace MeterReaderAPI.Accounts {
    public interface IAccountsSeedService {
        public Task SeedAsync(IEnumerable<Account> accounts, CancellationToken ct);
    }

    public class AccountsSeedService(ApplicationDbContext context, ILogger<AccountsSeedService> logger) : IAccountsSeedService {
        public async Task SeedAsync(IEnumerable<Account> accounts, CancellationToken ct) {
            var existingIds = await context.Accounts
                .Select(a => a.AccountId)
                .ToListAsync(ct);

            var newAccounts = accounts
                .Where(a => !existingIds.Contains(a.AccountId))
                .ToList();

            if (newAccounts.Count != 0) {
                logger.LogDebug("Found {count} of {totalAccounts} new accounts to add", newAccounts.Count, accounts.Count());
                await context.Accounts.AddRangeAsync(newAccounts, ct);
                await context.SaveChangesAsync(ct);
                logger.LogInformation("Added {accountsAdded} of {totalAccounts} new accounts to database", newAccounts.Count, accounts.Count());
            } else {
                logger.LogInformation("Found {totalAccounts} accounts in CSV, all of which were already ingested", accounts.Count());
            }
        }
    }
}
