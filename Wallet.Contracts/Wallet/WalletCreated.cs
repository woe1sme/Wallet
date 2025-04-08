namespace Wallet.Contracts.Wallet;

public record WalletCreated(Guid Id, string WalletName) : IContractMessage;
