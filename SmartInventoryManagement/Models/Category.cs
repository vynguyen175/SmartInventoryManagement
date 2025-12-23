using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using SmartInventoryManagement.Models;

namespace SmartInventoryManagement.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Category name is required.")]
        [StringLength(100, ErrorMessage = "Category name cannot exceed 100 characters.")]
        public string Name { get; set; }

        // Use string for the Description to be mapped as TEXT in SQLite
        public string Description { get; set; }

        // Navigation property for related products
        [Required]
        public ICollection<Product> Products { get; set; }

        // Constructor to initialize collections
        public Category()
        {
            Name = string.Empty;
            Description = string.Empty;
            Products = new List<Product>();
        }
    }
}