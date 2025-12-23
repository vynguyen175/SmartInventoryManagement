using System.ComponentModel.DataAnnotations;
using SmartInventoryManagement.Models;

namespace SmartInventoryManagement.Models
{
    public class Order
    {
        public int Id { get; set; }

        [Required] public string GuestName { get; set; }

        [Required] [EmailAddress] public string GuestEmail { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Total price must be greater than zero.")]
        public decimal TotalPrice { get; set; }

        [Required] public DateTime CreatedDate { get; set; } = DateTime.UtcNow; // ✅ Store as UTC

        // ✅ Navigation property
        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        
        public string CreatedBy { get; set; } // "Guest" or user's email/username

        // ✅ Add a parameterless constructor to avoid errors
        public Order()
        {
        }
        
        public Order(string guestName, string guestEmail, decimal totalPrice)
        {
            GuestName = guestName;
            GuestEmail = guestEmail;
            TotalPrice = totalPrice;
            CreatedDate = DateTime.UtcNow;
            OrderItems = new List<OrderItem>();
        }
    }
}