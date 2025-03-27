using System;
using System.Collections.Generic;

namespace BookingTour.Models;

public partial class Blog
{
    public long IdBlog { get; set; }

    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;

    public string Id { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string Status { get; set; } = null!;

    public string? ThumbnailUrl { get; set; }

    public string? ImageUrl { get; set; }

    public string? Tags { get; set; }
}
