using System.ComponentModel.DataAnnotations;

namespace ChitFundManager.Models
{
    public class UpdateMemberDto
    {
        [StringLength(100, MinimumLength = 3)]
        public string? Name { get; set; }

        [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Enter a valid 10-digit Indian phone number")]
        public string? PhoneNumber { get; set; }

        [StringLength(250)]
        public string? Address { get; set; }
    }
}