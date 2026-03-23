using System;
using System.ComponentModel.DataAnnotations;

namespace ChitFundManager.Models
{
public class MemberPaymentHistoryDto
{
    public int MonthNumber { get; set; }
    public decimal AmountToPay { get; set; }   // remaining
    public decimal AmountPaid { get; set; }
    public bool IsPaid { get; set; }
    public DateTime? PaidDate { get; set; }
}
}