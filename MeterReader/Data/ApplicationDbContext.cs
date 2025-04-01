using MeterReaderAPI.Accounts;
using Microsoft.EntityFrameworkCore;

namespace MeterReaderAPI.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options) {
    public DbSet<Account> Accounts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Account>(entity => {
            entity.HasKey(e => e.AccountId);
            entity.Property(e => e.FirstName).IsRequired();
            entity.Property(e => e.LastName).IsRequired();
        });
    }
}
