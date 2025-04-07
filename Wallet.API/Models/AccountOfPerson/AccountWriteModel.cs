using Wallet.API.Models.Base;

namespace Wallet.API.Models.AccountOfPerson
{
    public record AccountWriteModel : FinancialWriteModel
    {
        public required string Description { get; init; }
        public required Guid ProfileId { get; init; }
    }
}
