namespace Wallet.Contracts.Wallet;

public record WalletUpdated(Guid Id, string WalletName): IContractMessage;
