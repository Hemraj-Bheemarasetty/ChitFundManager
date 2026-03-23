using System;
using System.ComponentModel.DataAnnotations;

namespace ChitFundManager.Models
{
public class MarkPaymentDto
{
    public Guid ChitGroupId { get; set; }
    public Guid MemberId { get; set; }
    public int MonthNumber { get; set; }
    public decimal AmountPaid { get; set; }
}
}