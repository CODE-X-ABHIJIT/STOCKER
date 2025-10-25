using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace STOCKER.Models
{
    public class InvoiceDetail
    {
        [Key]
        public int InvoiceDetailId { get; set; }

        [ForeignKey("Invoice")]
        public int InvoiceId { get; set; }

        public SalesInvoice Invoice { get; set; }

        [ForeignKey("ProductId")]
        public int ProductId { get; set; }

        public Products Products { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [NotMapped]
        public decimal Total => Quantity * UnitPrice;
    }
}
