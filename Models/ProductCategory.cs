using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace STOCKER.Models
{
    public class ProductCategory
    {
        [Key] // This defines CategoryId as the primary key
        public int CategoryId { get; set; }

        [Required]
        public string Name { get; set; }

        [ValidateNever]
        public ICollection<Products> Products { get; set; }
    }
}
