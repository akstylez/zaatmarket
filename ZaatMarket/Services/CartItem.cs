namespace ZaatMarket.Cart;

public class CartItem
{
    public int ProductId { get; set; }
    public string Title { get; set; } = "";
    public decimal Price { get; set; }
    public int Quantity { get; set; } = 1;

    public decimal LineTotal => Price * Quantity;
}