using Family.Contracts.FamilyMember;
using MassTransit;
using Wallet.API.Consumers.FamilyConsumers;

namespace Wallet.API.Consumers.FamilyMemberConsumers
{
    public class FamilyMemberCreatedConsumer : IConsumer<FamilyMemberCreated>
    {
        private readonly ILogger<FamilyMemberCreatedConsumer> _logger;

        public FamilyMemberCreatedConsumer(ILogger<FamilyMemberCreatedConsumer> logger)
        {
            _logger = logger;
        }
        public Task Consume(ConsumeContext<FamilyMemberCreated> context)
        {
            _logger.LogInformation($"FamilyMemberCreatedConsumer Consume: {context.Message.ToString()}");

            //TO-DO no family member create functionality provided yet

            return Task.CompletedTask;
        }
    }
}
