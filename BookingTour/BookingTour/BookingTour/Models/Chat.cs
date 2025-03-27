using System;
using System.Collections.Generic;

namespace BookingTour.Models;

public partial class Chat
{
    public long IdChat { get; set; }

    public string? Message { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string Id { get; set; } = null!;

    public virtual AspNetUser IdNavigation { get; set; } = null!;
}
