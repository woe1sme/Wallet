using Wallet.Domain.SeedWork;

namespace Wallet.Domain.Models.BaseEntity;

public abstract class Owner : Entity<Guid>
{
    public Owner(Guid id, string name) : base(id)
    {
        Name = name;
    }
    public string Name { get; set; }
}