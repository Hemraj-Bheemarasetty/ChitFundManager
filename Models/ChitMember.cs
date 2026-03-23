namespace ChitFundManager.Models
{
public class ChitMember
{
    public Guid Id { get; set; }

    public Guid ChitGroupId { get; set; }
    public ChitGroup ChitGroup { get; set; }

    public Guid MemberId { get; set; }
    public Member Member { get; set; }

    public bool HasWon { get; set; } = false;
    public int? WinningMonth { get; set; }
    public decimal? WinningAmount { get; set; }
}
}