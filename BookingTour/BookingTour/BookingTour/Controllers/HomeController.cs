using BookingTour.Data;
using BookingTour.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
namespace BookingTour.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
		private readonly YourExistingDbContextName _context;
		public HomeController(ILogger<HomeController> logger, YourExistingDbContextName context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new SearchTourViewModel
            {
                Destinations = await _context.Destinations.ToListAsync(),
                TourTypes = await _context.TypeOfTours.ToListAsync()
            };

            return View(viewModel);

        }
        public IActionResult Index1()
        {
            return View();
        }
        public IActionResult AccessDenied()
        {
            return View();
        }
        public IActionResult natrz()
		{
			return View();
		}

		public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
