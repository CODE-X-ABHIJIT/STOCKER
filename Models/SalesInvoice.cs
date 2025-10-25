using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace STOCKER.Models
{
    public class SalesInvoice
    {
        [Key]
        public int InvoiceId { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public IdentityUser User { get; set; }

        [Required]
        public string CustomerName { get; set; }

        [Required]
        public string CustomerMobile { get; set; }

        [Required]
        public string CustomerAddress { get; set; }
        [Required]
        public string PaymentStatus { get; set; } // "Paid" or "Unpaid"
        public DateTime? PaidDate { get; set; } // Nullable: only filled if status is "Paid"

        public ICollection<InvoiceDetail> InvoiceDetails { get; set; } = new List<InvoiceDetail>();
    }
}
