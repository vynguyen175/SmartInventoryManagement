using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;


namespace SmartInventoryManagement.Models
{
    public class ProfileViewModel
    {
        [Required]
        public string FullName { get; set; }

        [BindNever]
        public string Email { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public string Pronouns { get; set; }

        public string Address { get; set; }
        
        [BindNever]
        public List<Order> Orders { get; set; }
    }
}