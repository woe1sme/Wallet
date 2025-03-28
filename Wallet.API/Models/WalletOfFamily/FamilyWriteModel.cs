using Wallet.API.Models.Base;

namespace Wallet.API.Models.WalletOfFamily;

public record FamilyWriteModel : IWriteModel
{
    public Guid Id { get; set; }
    public required string Name { get; init; }
    public required HeadMemberWriteModel HeadMember { get; set; }
}
