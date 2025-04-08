namespace Wallet.Contracts.Account;

public record AccountUpdated(Guid Id, string Description) : IContractMessage;
