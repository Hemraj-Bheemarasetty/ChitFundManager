namespace ChitFundManager.Models
{
public class Auction
{
    public Guid Id { get; set; }

    public Guid ChitGroupId { get; set; }
    public ChitGroup ChitGroup { get; set; }

    public int MonthNumber { get; set; }

    public Guid WinnerMemberId { get; set; }
    public Member WinnerMember { get; set; }


    public decimal BidAmount { get; set; }            // 20000
    public decimal WinnerAmount { get; set; }         // 80000
    public decimal CommissionAmount { get; set; }     // 3000
    public decimal TotalCollection { get; set; }      // 83000
    public decimal MonthlyPayablePerMember { get; set; } // 4150

    public DateTime AuctionDate { get; set; }
}
}