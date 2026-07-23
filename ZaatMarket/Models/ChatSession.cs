using System.ComponentModel.DataAnnotations;

namespace ZaatMarket.Models;

public class ChatSession
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string UserId { get; set; } = string.Empty;
    public string Title { get; set; } = "New Chat";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // CHANGED from ChatMessage to AiMessage
    public List<AiMessage> Messages { get; set; } = new();
}