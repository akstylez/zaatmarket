using System.ComponentModel.DataAnnotations;

namespace ZaatMarket.Models
{
    public class TelemetryEvent
    {
        [Key]
        public int Id { get; set; }

        // e.g., "PURCHASE", "SEARCH", "MESSAGE_SENT", "ITEM_VIEW"
        public string ActionType { get; set; } = string.Empty;

        // e.g., "User clicked on MacBook Pro", "Payment of $300 cleared"
        public string Description { get; set; } = string.Empty;

        public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;

        // e.g., IP Address, Geo-location, or exact JSON data
        public string? Metadata { get; set; }
    }
}