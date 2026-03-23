using System;
using System.ComponentModel.DataAnnotations;

namespace ChitFundManager.Models
{
public class AuctionResponseDto
{
    public Guid Id { get; set; }
    public int MonthNumber { get; set; }
    public Guid WinnerMemberId { get; set; }
    public decimal TotalCollection { get; set; }
    public decimal BidAmount { get; set; }
    public decimal WinnerAmount { get; set; }
    public decimal MonthlyPayablePerMember { get; set; }
    public decimal CommissionAmount { get; set; }
    public DateTime AuctionDate { get; set; }
}
}