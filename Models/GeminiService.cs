using System.Text;
using System.Text.Json;

namespace TMS.Services
{
    public class GeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _model;

        public GeminiService(IConfiguration config)
        {
            _httpClient = new HttpClient();
            _apiKey = config["Gemini:ApiKey"] ?? throw new ArgumentNullException("Gemini:ApiKey not configured");
            _model =  "gemini-2.5-flash";
        }

        public async Task<string> SendPromptAsync(string prompt)
        {
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}";

            var body = new
            {
                contents = new[]
                {
                    new {
                        parts = new[] { new { text = prompt } }
                    }
                }
            };

            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);
            var result = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Gemini API error: {result}");
            }

            using var doc = JsonDocument.Parse(result);
            var candidates = doc.RootElement.GetProperty("candidates");

            var sb = new StringBuilder();
            foreach (var candidate in candidates.EnumerateArray())
            {
                var parts = candidate.GetProperty("content").GetProperty("parts");
                foreach (var part in parts.EnumerateArray())
                {
                    if (part.TryGetProperty("text", out var textElement))
                    {
                        sb.AppendLine(textElement.GetString());
                    }
                }
            }

            return sb.ToString().Trim();
        }
    }
}
