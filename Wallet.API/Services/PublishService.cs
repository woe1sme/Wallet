using MassTransit;
using Wallet.API.Services.Abstractions;
using Wallet.Contracts;

namespace Wallet.API.Services;

public class PublishService : IPublishService
{
    private readonly IPublishEndpoint _walletPublishEndpoint;

    public PublishService(IPublishEndpoint walletPublishEndpoint) 
    {
        _walletPublishEndpoint = walletPublishEndpoint;
    }

    public async Task PublishAsync(IContractMessage message)
    {
        await _walletPublishEndpoint.Publish(message);
    }
}
