using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace STOCKER.Models
{
    public class Products
    {
        [Key]
        public int ProductId { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        public decimal BuyingPrice { get; set; }

        [Required] 
        public int ProductCategoryId { get; set; }

        [ForeignKey("ProductCategoryId")]
        [ValidateNever] 
        public ProductCategory Category { get; set; }

        [ValidateNever] 
        public Inventory Inventory { get; set; }
    }
}
