namespace ChitFundManager.Models
{
public class Member
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string PhoneNumber { get; set; }
    public string Address { get; set; }

    public ICollection<ChitMember> ChitMemberships { get; set; }
    public ICollection<Payment> Payments { get; set; }
}
}