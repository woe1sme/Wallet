using Wallet.API.Models.Base;

namespace Wallet.API.Models.AccountOfPerson
{
    public record AccountReadModel : FinancialReadModel
    {
        public required string Description { get; init; }
        public required Guid ProfileId { get; init; }
    }
}
