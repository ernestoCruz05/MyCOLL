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
        /// Add a product to the cart (respects stock limits)
        /// </summary>
        public bool AddItem(Produto product, int quantity = 1)
        {
            if (product.Stock <= 0) return false;
            
            var existing = _items.FirstOrDefault(i => i.Product.Id == product.Id);
            if (existing != null)
            {
                // Verificar se a nova quantidade excede o stock
                int newQuantity = existing.Quantity + quantity;
                if (newQuantity > product.Stock)
                {
                    // Limitar ao stock disponível
                    existing.Quantity = product.Stock;
                }
                else
                {
                    existing.Quantity = newQuantity;
                }
            }
            else
            {
                // Verificar se a quantidade inicial não excede o stock
                int actualQuantity = Math.Min(quantity, product.Stock);
                _items.Add(new CartItem
                {
                    Product = product,
                    Quantity = actualQuantity
                });
            }
            NotifyCartChanged();
            return true;
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
        /// Update quantity of an item (respects stock limits)
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
                    // Limitar ao stock disponível
                    item.Quantity = Math.Min(quantity, item.Product.Stock);
                }
                NotifyCartChanged();
            }
        }

        /// <summary>
        /// Get available quantity that can still be added for a product
        /// </summary>
        public int GetAvailableQuantity(int productId, int productStock)
        {
            var currentInCart = GetQuantity(productId);
            return Math.Max(0, productStock - currentInCart);
        }

        /// <summary>
        /// Check if more items can be added to cart for a product
        /// </summary>
        public bool CanAddMore(int productId, int productStock)
        {
            return GetAvailableQuantity(productId, productStock) > 0;
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
