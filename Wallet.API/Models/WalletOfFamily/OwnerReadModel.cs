using Wallet.API.Models.Base;

namespace Wallet.API.Models.WalletOfFamily
{
    public record OwnerReadModel : IReadModel<Guid>
    {
        public required Guid Id { get; init; }
        public string? Name { get; init; }
    }
}
