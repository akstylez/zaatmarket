namespace ZaatMarket.Cart;

public class CartState
{
    public List<CartItem> Items { get; set; } = new();

    public int TotalItems => Items.Sum(i => i.Quantity);
    public decimal Subtotal => Items.Sum(i => i.LineTotal);
}