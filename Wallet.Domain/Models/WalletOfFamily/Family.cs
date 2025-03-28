using Wallet.Domain.Exceptions;
using Wallet.Domain.SeedWork;

namespace Wallet.Domain.Models.WalletOfFamily;

public class Family : Entity<Guid>
{
    public Family(Guid id, string name) : base(id)
    {
        Name = name;
    }

    public string Name { get; protected set; }
    public virtual HeadMember HeadMember { get; protected set; }

    public virtual void AddHeadMember(HeadMember? headMember)
    {
        HeadMember = headMember ?? throw new ArgumentNullException($"Property {nameof(headMember)} cannot be null");
    }

    public virtual void ChangeName(string name)
    {
        Name = name;
    }
}