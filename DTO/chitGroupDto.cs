using System;
using System.ComponentModel.DataAnnotations;

namespace ChitFundManager.Models
{
public class ChitGroupDto
{
    [Required]
    public string ChitName { get; set; }

    [Required]
    public decimal TotalAmount { get; set; }

    [Required]
    public int TotalMembers { get; set; }

    [Required]
    public int DurationMonths { get; set; }

    [Required]
    public decimal CommissionPercent { get; set; }

    

   
}
}