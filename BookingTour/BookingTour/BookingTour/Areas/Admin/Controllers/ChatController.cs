using BookingTour.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingTour.Areas.Admin.Controllers
{
    public class ChatController : Controller
    {
        [Area("Admin")]
        [Authorize(Roles = CD.Role_Admin)]
        public IActionResult Index()
        {
            return View();
        }
    }
}
