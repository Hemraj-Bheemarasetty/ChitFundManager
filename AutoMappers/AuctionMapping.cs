using AutoMapper;
using ChitFundManager.Models;
namespace ChitFundManager.AutoMappers
{
    public class AuctionMapping : Profile
    {
        public AuctionMapping()
        {
            CreateMap<Auction,CreateAuctionDto>();
            CreateMap<CreateAuctionDto,Auction>();
            CreateMap<AuctionResponseDto,Auction>();
            CreateMap<Auction,AuctionResponseDto>();
        }
    }
}