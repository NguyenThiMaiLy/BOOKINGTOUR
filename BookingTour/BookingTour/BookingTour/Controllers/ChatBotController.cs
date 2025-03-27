using BookingTour.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BookingTour.Controllers
{
    public class ChatBotController : Controller
    {
        private readonly DialogflowService _dialogflowService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ChatBotController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
            var jsonCredentialsPath = Path.Combine(Directory.GetCurrentDirectory(), "C:\\Users\\USER\\Downloads\\chatboxtour-6b6c45629870.json");
            _dialogflowService = new DialogflowService("chatboxtour", jsonCredentialsPath);
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] string userMessage)
        {
            // Tạo session ID ngẫu nhiên nếu người dùng không đăng nhập
            var sessionId = Guid.NewGuid().ToString();

            // Gọi Dialogflow để xử lý tin nhắn
            var response = await _dialogflowService.DetectIntentAsync(sessionId, userMessage);

            // Trả về phản hồi từ Dialogflow dưới dạng JSON
            return Json(new { response });
        }


        public IActionResult Index()
        {
            return View();
        }
    }

}