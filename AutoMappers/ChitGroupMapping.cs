using AutoMapper;
using ChitFundManager.Models;
namespace ChitFundManager.AutoMappers
{
    public class ChitGroupMapping : Profile
    {
        public ChitGroupMapping()
        {
            CreateMap<ChitGroup,ChitGroupDto>();
            CreateMap<ChitGroupDto,ChitGroup>();
            CreateMap<ChitGroupGetDto,ChitGroup>();
            CreateMap<ChitGroup,ChitGroupGetDto>();
            CreateMap<ChitGroup, ChitGroupGetDto>()
    .ForMember(dest => dest.Members,
        opt => opt.MapFrom(src => src.Members.Select(x => x.Member)));
        }
    }
}