using LedgerFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LedgerFlow.Persistence.Context;

public class LedgerFlowDbContext(DbContextOptions<LedgerFlowDbContext> options) : DbContext(options)
{
    public DbSet<Wallet> Wallets => Set<Wallet>();
    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Wallet>(entity =>
        {
            entity.ToTable("wallets");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Currency).HasMaxLength(3);
            entity.Property(x => x.Balance).HasColumnType("numeric(18,2)");
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.ToTable("transactions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Currency).HasMaxLength(3);
            entity.Property(x => x.Amount).HasColumnType("numeric(18,2)");
        });
    }
}
