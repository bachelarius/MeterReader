using LanguageExt;
using MeterReaderAPI.Accounts;
using Microsoft.EntityFrameworkCore;

namespace MeterReaderAPI.Data;

public static class DatabaseInitializationExtensions {
    public static async Task InitializeDatabaseAsync(this IServiceProvider services, CancellationToken ct) {
        using var scope = services.CreateScope();
        var provider = scope.ServiceProvider;

        var context = provider.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync(ct);

        var extractor = provider.GetRequiredService<IAccountsExtractorService>();
        var seeder = provider.GetRequiredService<IAccountsSeedService>();

        var accounts = extractor.ExtractAccounts();
        var asDomain = accounts.Select(a => a.ToDomain());
        await seeder.SeedAsync(asDomain, ct);
    }
}
