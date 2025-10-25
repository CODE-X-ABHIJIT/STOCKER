using System;
using System.Collections.Generic;

namespace STOCKER.Models.ViewModels
{
    public class SalesReport
    {
        public List<SalesInvoice> Invoices { get; set; }
        public string FilterStatus { get; set; }
        public string CustomerName { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public decimal TotalAmount => Invoices?.Sum(i => i.InvoiceDetails.Sum(d => d.Quantity * d.UnitPrice)) ?? 0;
        public decimal TotalUnpaid => Invoices?
            .Where(i => i.PaymentStatus == "Unpaid")
            .Sum(i => i.InvoiceDetails.Sum(d => d.Quantity * d.UnitPrice)) ?? 0;
        public decimal TotalPaid => Invoices?
            .Where(i => i.PaymentStatus == "Paid")
            .Sum(i => i.InvoiceDetails.Sum(d => d.Quantity * d.UnitPrice)) ?? 0;
    }
}
