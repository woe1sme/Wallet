namespace Wallet.API.Models.WalletOfFamily
{
    public record SubWalletTransferFundsWriteModel : WalletTransferFundsWriteModel
    {
        public Guid FamilyMemberId { get; set; }
    }
}
