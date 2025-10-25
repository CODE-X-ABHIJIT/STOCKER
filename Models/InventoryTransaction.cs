using System.ComponentModel.DataAnnotations.Schema;

namespace STOCKER.Models
{
    public class InventoryTransaction
    {
        public int InventoryTransactionId { get; set; }

        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public Products Product { get; set; }

        public int QuantityAdded { get; set; } // For "Add" type
        public int QuantitySold { get; set; }  // For "Sell" type

        public decimal BuyingPrice { get; set; }  // For "Add"
        public decimal SellingPrice { get; set; } // For "Sell"

        public string Type { get; set; } // "Add" or "Sell"
        public DateTime Timestamp { get; set; }

        public int? SalesInvoiceId { get; set; }
        public SalesInvoice SalesInvoice { get; set; }
    }


}
