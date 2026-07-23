using System;

namespace ZaatMarket.Models
{
    public class Order
    {
        public int Id { get; set; }

        // Tracking details
        public string Reference { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        // Financials
        public decimal TotalAmount { get; set; }

        // Customer Info
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;

        // Logistics
        public string DeliveryFrom { get; set; } = string.Empty;
        public string DeliveryTo { get; set; } = string.Empty;
    }
}