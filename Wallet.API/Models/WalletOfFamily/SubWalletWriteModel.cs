namespace Wallet.API.Models.WalletOfFamily;

public record SubWalletWriteModel : WalletWriteModel
{
    public required Guid ParentWalletId { get; init; }
    public List<FamilyMemberWriteModel>? FamilyMembers { get; init; }
}
