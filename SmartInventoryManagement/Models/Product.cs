using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartInventoryManagement.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Product name is required.")]
        [StringLength(100, ErrorMessage = "Product name cannot exceed 100 characters.")]
        public required string Name { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
        public decimal Price { get; set; }

        public int CategoryId { get; set; }
        
        [ForeignKey("CategoryId")]
        public Category? Category { get; set; }

        // ✅ Fix: Change from `OrderProduct` to `OrderItem`
        public List<OrderItem> OrderItems { get; set; } = new();

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Stock quantity cannot be negative.")]
        public int QuantityInStock { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Low stock threshold must be at least 1.")]
        public int LowStockThreshold { get; set; }

        public Product()
        {
            Name = string.Empty;
        }
    }
}