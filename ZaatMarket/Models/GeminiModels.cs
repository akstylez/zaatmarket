namespace ZaatMarket.Models
{
    // The structure we SEND to Gemini
    public class GeminiRequest
    {
        public List<GeminiContent> contents { get; set; } = new();
    }

    public class GeminiContent
    {
        public string role { get; set; } = "user"; // Can be "user" or "model"
        public List<GeminiPart> parts { get; set; } = new();
    }

    public class GeminiPart
    {
        public string text { get; set; } = "";
    }

    // The structure we RECEIVE from Gemini
    public class GeminiResponse
    {
        public List<GeminiCandidate>? candidates { get; set; }
    }

    public class GeminiCandidate
    {
        public GeminiContent? content { get; set; }
    }

    // Shared class for the UI and Service to track chat history
}