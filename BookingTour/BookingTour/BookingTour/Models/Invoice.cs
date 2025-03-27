using System;
using System.Collections.Generic;

namespace BookingTour.Models;

public partial class Invoice
{
    public long IdInvoice { get; set; }

    public string? Description { get; set; }

    public decimal? TotalAmount { get; set; }

    public DateTime? BillingDate { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<PaymentInvoice> PaymentInvoices { get; set; } = new List<PaymentInvoice>();
}
