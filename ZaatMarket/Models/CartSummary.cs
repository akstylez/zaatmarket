using System.Linq;

namespace ZaatMarket.Models
{
    public class CartSummary
    {
        public int Count { get; set; }
        public decimal Subtotal { get; set; }

        public static CartSummary FromItems(IEnumerable<CartItem> items)
        {
            var list = items.ToList();

            return new CartSummary
            {
                Count = list.Sum(x => x.Quantity),
                Subtotal = list.Sum(x => x.LineTotal)
            };
        }
    }
}