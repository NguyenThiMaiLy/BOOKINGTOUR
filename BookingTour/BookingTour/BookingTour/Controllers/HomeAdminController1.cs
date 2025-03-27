using Microsoft.AspNetCore.Mvc;

namespace BookingTour.Controllers
{
	public class HomeAdminController1 : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
	}
}
