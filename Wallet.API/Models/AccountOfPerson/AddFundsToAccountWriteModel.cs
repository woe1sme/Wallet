using Wallet.API.Models.Base;

namespace Wallet.API.Models.AccountOfPerson
{
    public class AddFundsToAccountWriteModel : IWriteModel
    {
        public Guid AccountId { get; set; }
        public decimal Amount{ get; set; } 
        public Guid ProfileId { get; set; }
    }
}
