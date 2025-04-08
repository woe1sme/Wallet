using Wallet.Contracts;

namespace Wallet.API.Services.Abstractions;

public interface IPublishService
{
    public Task PublishAsync(IContractMessage message);
}