using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace BookingTour.Controllers
{
    public class ChatController : Controller
    {
        private readonly string apiKey;
        private readonly string apiUrl = "https://api.openai.com/v1/engines/davinci-codex/completions"; // URL của API OpenAI

        public ChatController(IConfiguration configuration)
        {
            // Lấy API key từ appsettings.json
            apiKey = configuration["OpenAI:ApiKey"];
        }

        [HttpPost]
        public async Task<IActionResult> GetResponse(string userMessage)
        {
            if (string.IsNullOrEmpty(userMessage))
                return Json(new { success = false, message = "Tin nhắn không hợp lệ." });

            var prompt = $"User: {userMessage}\nAI:";
            var data = new
            {
                prompt = prompt,
                max_tokens = 100
            };

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            var json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(apiUrl, content);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                dynamic aiResponse = JsonConvert.DeserializeObject(result);
                string botMessage = aiResponse.choices[0].text;
                return Json(new { success = true, message = botMessage });
            }
            else
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi gửi tin nhắn." });
            }
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}