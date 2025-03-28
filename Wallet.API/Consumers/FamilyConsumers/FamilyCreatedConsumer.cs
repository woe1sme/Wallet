using Family.Contracts.Familly;
using MassTransit;
using Wallet.API.Models.WalletOfFamily;
using Wallet.API.Services.Abstractions;

namespace Wallet.API.Consumers.FamilyConsumers;

public class FamilyCreatedConsumer : IConsumer<FamilyCreated>
{
    private readonly IWalletService _walletService;
    private readonly ILogger<FamilyCreatedConsumer> _logger;

    public FamilyCreatedConsumer(IWalletService walletService, ILogger<FamilyCreatedConsumer> logger)
    {
        _walletService = walletService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<FamilyCreated> context)
    {
        _logger.LogInformation($"FamilyCreatedConsumer Consume: {context.Message.ToString()}");

        var familyWriteModel = new FamilyWriteModel
        {
            Id = context.Message.FamilyId,
            Name = context.Message.FamilyName,
            HeadMember = new HeadMemberWriteModel 
            { 
                Id = context.Message.FamilyHeadUserId, 
                Name = context.Message.FamilyName 
            }
        };

        var walletWriteModel = new WalletWriteModel
        {
            Family = familyWriteModel,
            Description = $"{familyWriteModel.Name}, {DateTime.UtcNow.ToShortDateString()}"
        };

        await _walletService.CreateWallet(walletWriteModel, CancellationToken.None);
    }
}
