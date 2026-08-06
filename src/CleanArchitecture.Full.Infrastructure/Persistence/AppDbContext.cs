using CleanArchitecture.Full.Domain;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Full.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Account> Accounts => Set<Account>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.ToTable("accounts");
            entity.HasKey(a => a.Id);
            entity.Property(a => a.AccountNumber).IsRequired().HasMaxLength(20);
            entity.Property(a => a.HolderName).IsRequired().HasMaxLength(150);
            entity.Property(a => a.Balance).HasColumnType("numeric(18,2)");
            entity.Property(a => a.Status).IsRequired().HasMaxLength(20);
            entity.HasIndex(a => a.AccountNumber).IsUnique();
        });
    }
}
