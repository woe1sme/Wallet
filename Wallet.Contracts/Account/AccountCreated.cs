namespace Wallet.Contracts.Account;

public record AccountCreated(Guid Id, string Description, Guid ProfileId) : IContractMessage;
