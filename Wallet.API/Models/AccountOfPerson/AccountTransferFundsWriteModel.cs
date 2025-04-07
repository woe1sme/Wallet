using Wallet.API.Models.Base;

namespace Wallet.API.Models.AccountOfPerson;

public record AccountTransferFundsWriteModel : IWriteModel
{
    public Guid ProfileId { get; set; }
    public Guid FromAccountId { get; init; }
    public Guid ToWalletId { get; init; }
    public decimal Amount { get; init; }
}
