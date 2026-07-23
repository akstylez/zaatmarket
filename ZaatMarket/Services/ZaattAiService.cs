using System.Net.Http.Json;
using ZaatMarket.Models;

namespace ZaatMarket.Services
{
    public class ZaattAiService
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;

        // UPDATED: Now pointing to gemini-2.5-flash
        private const string Endpoint = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key=";

        public ZaattAiService(HttpClient http, IConfiguration config)
        {
            _http = http;
            _apiKey = config["GeminiApiKey"] ?? throw new Exception("Gemini API Key is missing in appsettings.json!");
        }

        public async Task<string> GetChatResponseAsync(List<AiMessage> chatHistory)
        {
            var requestPayload = new GeminiRequest();

            // 1. Inject the secret System Prompt first
            requestPayload.contents.Add(new GeminiContent
            {
                role = "user",
                parts = new List<GeminiPart> { new GeminiPart { text = "You are ZAATT AI, an advanced, professional, and helpful shopping assistant for the ZaatMarket digital marketplace. Keep answers brief, friendly, and highly relevant to ecommerce, buying, selling, and product navigation. Do not break character." } }
            });

            // 2. The model must acknowledge the system prompt to maintain the strict User/Model alternating flow
            requestPayload.contents.Add(new GeminiContent
            {
                role = "model",
                parts = new List<GeminiPart> { new GeminiPart { text = "Understood. I am ZAATT AI, ready to assist." } }
            });

            // 3. Map the live UI chat history
            foreach (var msg in chatHistory)
            {
                requestPayload.contents.Add(new GeminiContent
                {
                    role = msg.IsUser ? "user" : "model",
                    parts = new List<GeminiPart> { new GeminiPart { text = msg.Text } }
                });
            }

            try
            {
                var response = await _http.PostAsJsonAsync($"{Endpoint}{_apiKey}", requestPayload);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<GeminiResponse>();

                return result?.candidates?.FirstOrDefault()?.content?.parts?.FirstOrDefault()?.text
                       ?? "I'm sorry, I couldn't process that response.";
            }
            catch (Exception ex)
            {
                return $"Connection Error: {ex.Message}. Please check your API key and connection.";
            }
        }
    }
}