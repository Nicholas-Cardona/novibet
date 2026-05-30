using Microsoft.EntityFrameworkCore;
using Novibet.Data.Entities;

namespace Novibet.Data;

public class AppDbContext : DbContext
{
    public DbSet<WalletEntity> Wallets => Set<WalletEntity>();

    public DbSet<CurrencyRateEntity> CurrencyRates => Set<CurrencyRateEntity>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
}
