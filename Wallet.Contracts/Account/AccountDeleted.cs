namespace Wallet.Contracts.Account;

public record AccountDeleted(Guid Id) : IContractMessage;
