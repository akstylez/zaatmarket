namespace ZaatMarket.Models
{
    public class CartItem
    {
        public int ProductId { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string ImageUrl { get; set; } = string.Empty;

        public decimal LineTotal => Price * Quantity;
    }
}