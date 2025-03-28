using Microsoft.EntityFrameworkCore;
using Wallet.Domain.Models.AccountOfPerson;
using Wallet.Domain.Models.BaseEntity;
using Wallet.Domain.Models.WalletOfFamily;
using Wallet.Infrastructure.Configurations;
using Models = Wallet.Domain.Models.WalletOfFamily;

namespace Wallet.Infrastructure;

public class WalletDBContext : DbContext
{
    public DbSet<Models.Family> Families { get; set; }
    public DbSet<HeadMember> HeadMembers { get; set; }
    public DbSet<FamilyMember> FamilyMembers { get; set; }
    public DbSet<Models.Wallet> Wallets { get; set; }
    public DbSet<Models.SubWallet> SubWallets { get; set; }
    public DbSet<Account> Accounts { get; set; }
    public WalletDBContext(DbContextOptions<WalletDBContext> options) : base(options)
    {
        //Database.EnsureDeleted();
        Database.EnsureCreated();
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new FamilyConfiguration());
        modelBuilder.ApplyConfiguration(new FamilyMemberConfiguration());
        modelBuilder.ApplyConfiguration(new HeadMemberConfiguration());
        modelBuilder.ApplyConfiguration(new WalletConfiguration());
        modelBuilder.ApplyConfiguration(new SubWalletConfiguration());
        modelBuilder.ApplyConfiguration(new AccountConfiguration());
    }
}