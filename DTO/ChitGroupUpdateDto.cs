 using System;
using System.ComponentModel.DataAnnotations;

namespace ChitFundManager.Models
{
public class ChitGroupUpdateDto
{   
    
    public string? ChitName { get; set; }
    public decimal? TotalAmount { get; set; }
    public int? TotalMembers { get; set; }
    public int? DurationMonths { get; set; }
    public decimal? CommissionPercent { get; set; }
    public DateTime? StartDate { get; set; }
    public bool? IsActive { get; set; }
}
}
