using ZaatMarket.Models;

namespace ZaatMarket.Services
{
    public interface ICartService
    {
        IReadOnlyList<CartItem> Items { get; }
        CartSummary Summary { get; }
        event Action OnChange;

        // NEW: Required to load the cart from the browser
        Task LoadCartAsync();

        void AddToCart(Product product);
        void RemoveFromCart(int productId);
        void IncreaseQuantity(int productId);
        void DecreaseQuantity(int productId);
        void Clear();
    }
}