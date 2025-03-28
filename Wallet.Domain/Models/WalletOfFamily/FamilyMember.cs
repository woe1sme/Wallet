using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Domain.Models.BaseEntity;

namespace Wallet.Domain.Models.WalletOfFamily
{
    public class FamilyMember : Owner
    {
        public FamilyMember(Guid id, string name) : base(id, name)
        {
        }
    }
}
