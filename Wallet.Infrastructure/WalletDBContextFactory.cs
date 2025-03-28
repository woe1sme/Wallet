using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Options;
using Wallet.Infrastructure;

public class WalletDBContextFactory : IDesignTimeDbContextFactory<WalletDBContext>
{
    public WalletDBContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<WalletDBContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Port=5434;Database=WalletDb;Username=postgres;Password=postgres");
        //optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=walletdb;Username=postgres;Password=postgres");

        return new WalletDBContext(optionsBuilder.Options);
    }
}
