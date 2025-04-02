using LanguageExt;
using LanguageExt.Common;
using MeterReaderAPI.Data;

namespace MeterReaderAPI.Accounts {
    public interface IAccountsSeedService {
        public Task<Result<Unit>> SeedAsync(CancellationToken ct);
    }

    public class AccountsSeedService(ApplicationDbContext context) : IAccountsSeedService {
        public readonly ApplicationDbContext _context = context;

        public Task<Result<Unit>> SeedAsync(CancellationToken ct) {
            return Task.FromResult(new Result<Unit>(new NotImplementedException()));
        }
    }
}
