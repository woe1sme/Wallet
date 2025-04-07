namespace Wallet.API.Models.Base
{
    public abstract record FinancialReadModel : IReadModel<Guid>
    {
        public required Guid Id { get; init; }
        public required decimal Balance { get; init; }
        public required string Currency { get; init; }
    }
}
