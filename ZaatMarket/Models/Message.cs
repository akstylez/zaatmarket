using System;
using System.ComponentModel.DataAnnotations;

namespace ZaatMarket.Models
{
    public class Message
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string SenderId { get; set; } = string.Empty;

        public string MessageType { get; set; } = "Text"; // Can be: Text, Image, Video, Audio
        public string? MediaUrl { get; set; }

        [Required]
        public string ReceiverId { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        public DateTime SentAtUtc { get; set; } = DateTime.UtcNow;

        public bool IsRead { get; set; } = false;
    }
}