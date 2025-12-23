using Microsoft.AspNetCore.Identity;

namespace SmartInventoryManagement.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
        public string ContactInformation { get; set; }
        public string SecurityQuestion { get; set; }
        public string SecurityAnswer { get; set; } // You can hash this later for better security
        
        public DateTime? DateOfBirth { get; set; }

        public string Pronouns { get; set; }

        public string Address { get; set; }


    }
}