using System.ComponentModel.DataAnnotations;

namespace SmartInventoryManagement.Models
{
    public class ForgotPWView
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string SecurityQuestion { get; set; }

        [Required]
        public string SecurityAnswer { get; set; }
    }
}