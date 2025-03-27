using Google.Cloud.Dialogflow.V2;

namespace BookingTour.Models
{
    public class DialogflowService
    {
        private readonly string _projectId;
        private readonly SessionsClient _sessionsClient;

        public DialogflowService(string projectId, string credentialsPath)
        {
            _projectId = projectId;
            System.Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialsPath);
            _sessionsClient = SessionsClient.Create();
        }

        public async Task<string> DetectIntentAsync(string sessionId, string text)
        {
            var session = new SessionName(_projectId, sessionId); // Sử dụng sessionId của người dùng
            var queryInput = new QueryInput
            {
                Text = new TextInput { Text = text, LanguageCode = "en" } // Gửi văn bản từ người dùng
            };

            var response = await _sessionsClient.DetectIntentAsync(session, queryInput);
            return response.QueryResult.FulfillmentText; // Trả về phản hồi từ Dialogflow
        }
    }

}