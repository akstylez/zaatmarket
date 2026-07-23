using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using ZaatMarket.Models;

namespace ZaatMarket.Services
{
    public class CartService : ICartService
    {
        private readonly ProtectedLocalStorage _localStorage;
        private List<CartItem> _items = new();

        public event Action? OnChange;

        // Inject the browser storage service here
        public CartService(ProtectedLocalStorage localStorage)
        {
            _localStorage = localStorage;
        }

        public IReadOnlyList<CartItem> Items => _items.AsReadOnly();

        public CartSummary Summary => CartSummary.FromItems(_items);

        // --- NEW: Storage Methods ---

        public async Task LoadCartAsync()
        {
            try
            {
                var result = await _localStorage.GetAsync<List<CartItem>>("zaatmarket_cart");
                if (result.Success && result.Value != null)
                {
                    _items = result.Value;
                    NotifyStateChanged();
                }
            }
            catch
            {
                // Fails silently if storage is empty or user cleared their cookies
            }
        }

        private async void SaveCart()
        {
            try
            {
                await _localStorage.SetAsync("zaatmarket_cart", _items);
            }
            catch { }
        }

        // -----------------------------

        public void AddToCart(Product product)
        {
            var existing = _items.FirstOrDefault(x => x.ProductId == product.Id);

            if (existing == null)
            {
                _items.Add(new CartItem
                {
                    ProductId = product.Id,
                    Title = product.Title,
                    Price = product.Price,
                    Quantity = 1,
                    // If your Product model uses "ImagePath" instead of "ImageUrl", change it here!
                    ImageUrl = product.ImagePath
                });
            }
            else
            {
                existing.Quantity++;
            }

            SaveCart();
            NotifyStateChanged();
        }

        public void RemoveFromCart(int productId)
        {
            var item = _items.FirstOrDefault(x => x.ProductId == productId);
            if (item == null) return;

            _items.Remove(item);

            SaveCart();
            NotifyStateChanged();
        }

        public void IncreaseQuantity(int productId)
        {
            var item = _items.FirstOrDefault(x => x.ProductId == productId);
            if (item == null) return;

            item.Quantity++;

            SaveCart();
            NotifyStateChanged();
        }

        public void DecreaseQuantity(int productId)
        {
            var item = _items.FirstOrDefault(x => x.ProductId == productId);
            if (item == null) return;

            item.Quantity--;

            if (item.Quantity <= 0)
            {
                _items.Remove(item);
            }

            SaveCart();
            NotifyStateChanged();
        }

        public void Clear()
        {
            _items.Clear();
            SaveCart();
            NotifyStateChanged();
        }

        private void NotifyStateChanged()
        {
            OnChange?.Invoke();
        }
    }
}