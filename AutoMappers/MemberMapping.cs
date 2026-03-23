using AutoMapper;
using ChitFundManager.Models;
namespace ChitFundManager.AutoMappers
{
    public class MemberMapping : Profile
    {
        public MemberMapping()
        {
            CreateMap<Member,CreateMemberDto>();
            CreateMap<CreateMemberDto,Member>();
        }
    }
}