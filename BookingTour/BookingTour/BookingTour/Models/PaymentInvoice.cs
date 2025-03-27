using System;
using System.Collections.Generic;

namespace BookingTour.Models;

public partial class PaymentInvoice
{
    public long IdPayment { get; set; }

    public long IdInvoice { get; set; }

    public decimal? PaidAmount { get; set; }

    public DateTime? PaymentDate { get; set; }

    public string? Status { get; set; }

    public virtual Invoice IdInvoiceNavigation { get; set; } = null!;

    public virtual Payment IdPaymentNavigation { get; set; } = null!;
}
