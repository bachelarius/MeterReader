using LanguageExt;
using LanguageExt.Common;
using MeterReaderAPI.Accounts;
using Microsoft.EntityFrameworkCore;

namespace MeterReaderAPI.Data;

public static class DatabaseInitializationExtensions {
    public static async Task<Result<Unit>> InitializeDatabaseAsync(this IServiceProvider services, ILogger logger, CancellationToken ct) {
        using var scope = services.CreateScope();
        var provider = scope.ServiceProvider;

        var context = provider.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync(ct);

        var seeder = provider.GetRequiredService<IAccountsSeedService>();
        return await seeder.SeedAsync(ct);
    }
}
