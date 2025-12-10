using MyCOLL.UIComponents.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace MyCOLL.UIComponents.Services
{
    public class CollectionApiService
    {
        private readonly HttpClient _httpClient;

        public CollectionApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        #region Categories

        public async Task<List<Categoria>> GetCategoriesAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<Categoria>>("api/Categorias") ?? new();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching categories: {ex.Message}");
                return new List<Categoria>();
            }
        }

        #endregion

        #region Products

        public async Task<List<Produto>> GetProductsAsync(int? categoryId = null)
        {
            try
            {
                string url = "api/Produtos";
                if (categoryId.HasValue && categoryId.Value > 0)
                    url += $"?categoriaId={categoryId.Value}";

                return await _httpClient.GetFromJsonAsync<List<Produto>>(url) ?? new();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching products: {ex.Message}");
                return new List<Produto>();
            }
        }

        public async Task<Produto?> GetProductByIdAsync(int id)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<Produto>($"api/Produtos/{id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching product {id}: {ex.Message}");
                return null;
            }
        }

        public async Task<List<Produto>> SearchProductsAsync(string searchTerm)
        {
            try
            {
                var allProducts = await GetProductsAsync();
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return allProducts;

                return allProducts
                    .Where(p => p.Nome.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                               (p.Descricao?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false))
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error searching products: {ex.Message}");
                return new List<Produto>();
            }
        }

        #endregion

        #region Authentication

        public async Task<AuthResult> LoginAsync(string email, string password)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/Auth/login", new { Email = email, Password = password });
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                    return new AuthResult { Success = true, Token = result?.Token, Message = "Login successful" };
                }
                return new AuthResult { Success = false, Message = "Invalid credentials" };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login error: {ex.Message}");
                return new AuthResult { Success = false, Message = "Connection error" };
            }
        }

        public async Task<AuthResult> RegisterAsync(string nome, string email, string password)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/Auth/register", new { Nome = nome, Email = email, Password = password });
                if (response.IsSuccessStatusCode)
                {
                    return new AuthResult { Success = true, Message = "Registration successful" };
                }
                var error = await response.Content.ReadAsStringAsync();
                return new AuthResult { Success = false, Message = error };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Registration error: {ex.Message}");
                return new AuthResult { Success = false, Message = "Connection error" };
            }
        }

        #endregion

        #region Orders

        public async Task<OrderResult> CreateOrderAsync(OrderCreateDto order)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/Encomenda", order);
                if (response.IsSuccessStatusCode)
                {
                    return new OrderResult { Success = true, Message = "Order placed successfully" };
                }
                return new OrderResult { Success = false, Message = "Failed to place order" };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Order creation error: {ex.Message}");
                return new OrderResult { Success = false, Message = "Connection error" };
            }
        }

        public async Task<List<ModoEntrega>> GetDeliveryModesAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<ModoEntrega>>("api/ModoEntrega") ?? new();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching delivery modes: {ex.Message}");
                return new List<ModoEntrega>();
            }
        }

        #endregion
    }

    #region DTOs and Models

    public class AuthResult
    {
        public bool Success { get; set; }
        public string? Token { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        public string? Token { get; set; }
    }

    public class OrderResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class OrderCreateDto
    {
        public string UserId { get; set; } = string.Empty;
        public int ModoEntregaId { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
    }

    public class OrderItemDto
    {
        public int ProdutoId { get; set; }
        public int Quantidade { get; set; }
    }

    public class ModoEntrega
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public decimal Preco { get; set; }
        public string? Descricao { get; set; }
    }

    #endregion
}