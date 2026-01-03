using MyCOLL.UIComponents.Models;

namespace MyCOLL.UIComponents.Services
{
    public class CartService
    {
        private readonly List<CartItem> _items = new();

        public event Action? OnCartChanged;

        public IReadOnlyList<CartItem> Items => _items.AsReadOnly();

        public int ItemCount => _items.Sum(i => i.Quantity);

        public decimal TotalPrice => _items.Sum(i => i.TotalPrice);

        public bool AddItem(Produto product, int quantity = 1)
        {
            if (product.Stock <= 0) return false;

            var existing = _items.FirstOrDefault(i => i.Product.Id == product.Id);
            if (existing != null)
            {
                int newQuantity = existing.Quantity + quantity;
                existing.Quantity = newQuantity > product.Stock ? product.Stock : newQuantity;
            }
            else
            {
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

        public void RemoveItem(int productId)
        {
            var item = _items.FirstOrDefault(i => i.Product.Id == productId);
            if (item != null)
            {
                _items.Remove(item);
                NotifyCartChanged();
            }
        }

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
                    item.Quantity = Math.Min(quantity, item.Product.Stock);
                }
                NotifyCartChanged();
            }
        }

        public int GetAvailableQuantity(int productId, int productStock)
        {
            var currentInCart = GetQuantity(productId);
            return Math.Max(0, productStock - currentInCart);
        }

        public bool CanAddMore(int productId, int productStock)
        {
            return GetAvailableQuantity(productId, productStock) > 0;
        }

        public void ClearCart()
        {
            _items.Clear();
            NotifyCartChanged();
        }

        public bool ContainsProduct(int productId)
        {
            return _items.Any(i => i.Product.Id == productId);
        }

        public int GetQuantity(int productId)
        {
            return _items.FirstOrDefault(i => i.Product.Id == productId)?.Quantity ?? 0;
        }

        private void NotifyCartChanged()
        {
            OnCartChanged?.Invoke();
        }
    }

    public class CartItem
    {
        public Produto Product { get; set; } = new();
        public int Quantity { get; set; }
        public decimal TotalPrice => Product.Preco * Quantity;
    }
}
