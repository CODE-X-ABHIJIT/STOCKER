using System.ComponentModel.DataAnnotations;

namespace STOCKER.Models
{
    public class CartItem
    {
        [Key]
        public int CartItemId { get; set; }

        public int ProductId { get; set; }

        public string ProductName { get; set; }

        public decimal UnitPrice { get; set; }

        public int Quantity { get; set; }

        public decimal Total => UnitPrice * Quantity;
    }
}
