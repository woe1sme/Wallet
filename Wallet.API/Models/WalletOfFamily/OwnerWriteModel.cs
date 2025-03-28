using Wallet.API.Models.Base;

namespace Wallet.API.Models.WalletOfFamily;

public record OwnerWriteModel : IWriteModel
{
    public Guid Id { get; init; }
    public string Name { get; init; }
}
