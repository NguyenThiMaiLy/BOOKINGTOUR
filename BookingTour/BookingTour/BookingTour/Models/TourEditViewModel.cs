using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace BookingTour.Models
{
    public class TourEditViewModel
    {
        public Tour Tour { get; set; }
        public IEnumerable<SelectListItem> IdTypes { get; set; }
        public IEnumerable<SelectListItem> IdTrans { get; set; }
        public IEnumerable<SelectListItem> IdHotels { get; set; }
    }

}
