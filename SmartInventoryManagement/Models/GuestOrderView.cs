using System.ComponentModel.DataAnnotations;

namespace SmartInventoryManagement.Models
{
    public class GuestOrderView
    {
        [Key] // Not necessary if HasNoKey() is used in ApplicationDbContext.cs
        public int OrderDetailId { get; set; }

        [Required]
        public int OrderId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero.")]
        public decimal Price { get; set; }

        public string ProductName { get; set; } // ✅ Ensure this exists for the UI
        public string GuestName { get; set; }
        public string GuestEmail { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalPrice { get; set; }
    }
}