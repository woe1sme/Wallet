using Wallet.API.Models.Base;

namespace Wallet.API.Models.WalletOfFamily
{
    public record FamilyReadModel : IReadModel<Guid>
    {
        public required Guid Id { get; init; }
        public required string Name { get; init; }
        public HeadMemberReadModel? HeadMember { get; init; }
    }
}
