namespace ChitFundManager.Models
{
public class Payment
{
    public Guid Id { get; set; }

    public Guid ChitGroupId { get; set; }
    public ChitGroup ChitGroup { get; set; }

    public Guid MemberId { get; set; }
    public Member Member { get; set; }

    public int MonthNumber { get; set; }

    public decimal AmountToPay { get; set; }
    public decimal AmountPaid { get; set; }

    public bool IsPaid { get; set; } = false;
    public DateTime? PaidDate { get; set; }
}
}