using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Domain.Models.BaseEntity;

namespace Wallet.Domain.Models.WalletOfFamily
{
    public class HeadMember : FamilyMember
    {
        public HeadMember(Guid id, string name) : base(id, name)
        {
        }
    }
}
