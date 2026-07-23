using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ZaatMarket.Models;

public class AiMessage
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    // Foreign Key linking this message to the specific session
    public string ChatSessionId { get; set; } = string.Empty;

    public string Text { get; set; } = string.Empty;
    public bool IsUser { get; set; }
    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    [JsonIgnore]
    public ChatSession? Session { get; set; }
}