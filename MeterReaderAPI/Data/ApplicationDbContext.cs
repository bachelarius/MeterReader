using MeterReaderAPI.Accounts;
using MeterReaderAPI.MeterReadings;
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

        modelBuilder.Entity<MeterReading>(entity => {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AccountId).IsRequired();
            entity.Property(e => e.MeterReadingDateTime).IsRequired();
            entity.Property(e => e.MeterReadingValue).IsRequired();
            entity.HasOne<Account>().WithMany().HasForeignKey(e => e.AccountId);
            entity.HasIndex(e => new { e.AccountId, e.MeterReadingDateTime, e.MeterReadingValue })
                .IsUnique();
        });
    }
}
