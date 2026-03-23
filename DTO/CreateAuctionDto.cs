using System;
using System.ComponentModel.DataAnnotations;

namespace ChitFundManager.Models
{
public class CreateAuctionDto
{
    public Guid ChitGroupId { get; set; }

    public Guid WinnerMemberId { get; set; }

    public decimal BidAmount { get; set; }

}
}