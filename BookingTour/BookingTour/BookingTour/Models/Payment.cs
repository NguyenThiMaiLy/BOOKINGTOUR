using System;
using System.Collections.Generic;

namespace BookingTour.Models;

public partial class Payment
{
    public long IdPayment { get; set; }

    public DateTime? Date { get; set; }

    public string? Method { get; set; }

    public decimal? Amount { get; set; }

    public string? Status { get; set; }

    public string? IsRefunded { get; set; }

    public decimal? RefundAmount { get; set; }

    public DateTime? RefundDate { get; set; }

    public string Id { get; set; } = null!;

    public virtual AspNetUser IdNavigation { get; set; } = null!;

    public virtual ICollection<PaymentInvoice> PaymentInvoices { get; set; } = new List<PaymentInvoice>();
}
