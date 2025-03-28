using AutoMapper;
using Wallet.API.Models.WalletOfFamily;
using Wallet.Domain.Models.WalletOfFamily;

namespace Wallet.API.Mapping;

public class FamilyMapping : Profile
{
    public FamilyMapping()
    {
        CreateMap<Domain.Models.WalletOfFamily.Family, FamilyReadModel>()
            .MaxDepth(3);
        CreateMap<FamilyWriteModel, Domain.Models.WalletOfFamily.Family>();
        CreateMap<HeadMember, HeadMemberReadModel>()
            .MaxDepth(3);
        CreateMap<HeadMemberWriteModel, HeadMember>();
    }
}
