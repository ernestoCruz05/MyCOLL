using MyCOLL.UIComponents.Models;

namespace MyCOLL.UIComponents.Services
{
    /// <summary>
    /// Service to manage shopping cart state across the application
    /// </summary>
    public class CartService
    {
        private readonly List<CartItem> _items = new();

        /// <summary>
        /// Event triggered when cart changes
        /// </summary>
        public event Action? OnCartChanged;

        /// <summary>
        /// Get all items in the cart
        /// </summary>
        public IReadOnlyList<CartItem> Items => _items.AsReadOnly();

        /// <summary>
        /// Get total number of items in cart
        /// </summary>
        public int ItemCount => _items.Sum(i => i.Quantity);

        /// <summary>
        /// Get total price of all items
        /// </summary>
        public decimal TotalPrice => _items.Sum(i => i.TotalPrice);

        /// <summary>
        /// Add a product to the cart
        /// </summary>
        public void AddItem(Produto product, int quantity = 1)
        {
            var existing = _items.FirstOrDefault(i => i.Product.Id == product.Id);
            if (existing != null)
            {
                existing.Quantity += quantity;
            }
            else
            {
                _items.Add(new CartItem
                {
                    Product = product,
                    Quantity = quantity
                });
            }
            NotifyCartChanged();
        }

        /// <summary>
        /// Remove a product from the cart
        /// </summary>
        public void RemoveItem(int productId)
        {
            var item = _items.FirstOrDefault(i => i.Product.Id == productId);
            if (item != null)
            {
                _items.Remove(item);
                NotifyCartChanged();
            }
        }

        /// <summary>
        /// Update quantity of an item
        /// </summary>
        public void UpdateQuantity(int productId, int quantity)
        {
            var item = _items.FirstOrDefault(i => i.Product.Id == productId);
            if (item != null)
            {
                if (quantity <= 0)
                {
                    _items.Remove(item);
                }
                else
                {
                    item.Quantity = quantity;
                }
                NotifyCartChanged();
            }
        }

        /// <summary>
        /// Clear all items from cart
        /// </summary>
        public void ClearCart()
        {
            _items.Clear();
            NotifyCartChanged();
        }

        /// <summary>
        /// Check if a product is in the cart
        /// </summary>
        public bool ContainsProduct(int productId)
        {
            return _items.Any(i => i.Product.Id == productId);
        }

        /// <summary>
        /// Get quantity of a specific product in cart
        /// </summary>
        public int GetQuantity(int productId)
        {
            return _items.FirstOrDefault(i => i.Product.Id == productId)?.Quantity ?? 0;
        }

        private void NotifyCartChanged()
        {
            OnCartChanged?.Invoke();
        }
    }

    /// <summary>
    /// Represents an item in the shopping cart
    /// </summary>
    public class CartItem
    {
        public Produto Product { get; set; } = new();
        public int Quantity { get; set; }
        public decimal TotalPrice => Product.Preco * Quantity;
    }
}
