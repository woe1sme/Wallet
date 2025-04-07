using Wallet.API.Models.Base;

namespace Wallet.API.Models.WalletOfFamily;

/// <summary> 
/// Запрос для перевода средств между кошельками. 
/// </summary>
public record WalletTransferFundsWriteModel : IWriteModel
{
    public Guid FromWalletId { get; init; }
    public Guid ToWalletId { get; init; }
    public decimal Amount { get; init; }
}
