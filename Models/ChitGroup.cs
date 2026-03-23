namespace ChitFundManager.Models
{
public class ChitGroup
{
    public Guid Id { get; set; }
    public string ChitName { get; set; }
    public decimal TotalAmount { get; set; }      // 100000
    public int TotalMembers { get; set; }         // 20
    public int DurationMonths { get; set; }       // 20
    public decimal CommissionPercent { get; set; } // 3
    public DateTime StartDate { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<ChitMember> Members { get; set; }
    public ICollection<Auction> Auctions { get; set; }
    public ICollection<Payment> Payments { get; set; }
}
}