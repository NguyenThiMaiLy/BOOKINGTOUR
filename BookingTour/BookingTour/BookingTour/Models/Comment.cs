using System;
using System.Collections.Generic;

namespace BookingTour.Models;

public partial class Comment
{
    public long IdTour { get; set; }

    public string Id { get; set; } = null!;

    public string? Content { get; set; }

    public DateTime? CommentDate { get; set; }

    public string? Reply { get; set; }
    public bool IsHidden { get; set; }  
    public int? Rating { get; set; }
    public virtual AspNetUser IdNavigation { get; set; } = null!;

    public virtual Tour IdTourNavigation { get; set; } = null!;
}
