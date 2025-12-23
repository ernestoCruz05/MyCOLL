using MyCOLL.UIComponents.Models; 
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Forms;
using System.Net.Http.Headers;

namespace MyCOLL.UIComponents.Services
{
    public class CollectionApiService
    {
        private readonly HttpClient _httpClient;

        public CollectionApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Retorna o URL base da API
        /// </summary>
        public string GetBaseUrl()
        {
            return _httpClient.BaseAddress?.ToString() ?? string.Empty;
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

        private void FixImageUrl(Produto p)
        {
            if (string.IsNullOrEmpty(p.ImagemUrl)) return;

            var currentBaseUrl = GetBaseUrl().TrimEnd('/');

            if (p.ImagemUrl.StartsWith("http"))
            {
                if (currentBaseUrl.Contains("10.0.2.2") && p.ImagemUrl.Contains("localhost"))
                {
                    p.ImagemUrl = p.ImagemUrl.Replace("localhost", "10.0.2.2");
                }
            }
            else
            {
                p.ImagemUrl = $"{currentBaseUrl}/{p.ImagemUrl.TrimStart('/')}";
            }
        }

        public async Task<List<Produto>> GetProductsAsync(int? categoryId = null)
        {
            try
            {
                string url = "api/Produtos";
                if (categoryId.HasValue && categoryId.Value > 0)
                    url += $"?categoriaId={categoryId.Value}";

                var products = await _httpClient.GetFromJsonAsync<List<Produto>>(url) ?? new();

                foreach (var p in products) FixImageUrl(p);

                return products;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return new List<Produto>();
            }
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/Produtos/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting product: {ex.Message}");
                return false;
            }
        }

        public async Task<Produto?> GetProductByIdAsync(int id)
        {
            try
            {
                var product = await _httpClient.GetFromJsonAsync<Produto>($"api/Produtos/{id}");
                if (product != null) FixImageUrl(product);
                return product;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
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

        public async Task<List<Produto>> GetMyProductsAsync()
        {
            try
            {
                var products = await _httpClient.GetFromJsonAsync<List<Produto>>("api/Produtos/meus") ?? new();
                foreach (var p in products) FixImageUrl(p);
                return products;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching my products: {ex.Message}");
                return new List<Produto>();
            }
        }


        public async Task<bool> CreateProductAsync(ProdutoCreateDto produto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/Produtos", produto);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating product: {ex.Message}");
                return false;
            }
        }

        public async Task<string?> UploadImageAsync(IBrowserFile file)
        {
            try
            {
                long maxFileSize = 1024 * 1024 * 5;

                using var content = new MultipartFormDataContent();
                using var fileContent = new StreamContent(file.OpenReadStream(maxFileSize));
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

                content.Add(fileContent, "file", file.Name);

                var response = await _httpClient.PostAsync("api/Upload/produto", content);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<UploadResult>();
                    // Tenta ler ImageUrl (novo padrão) ou Url (fallback)
                    return result?.ImageUrl ?? result?.Url;
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Upload error: {ex.Message}");
                return null;
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

        public async Task<AuthResult> RegisterAsync(string email, string password, string confirmPassword)
        {
            try
            {
                var payload = new { Email = email, Password = password, ConfirmPassword = confirmPassword };

                var response = await _httpClient.PostAsJsonAsync("api/Auth/register", payload);

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

        public async Task<AuthResult> RegisterFornecedorAsync(string email, string password, string confirmPassword)
        {
            try
            {
                var payload = new { Email = email, Password = password, ConfirmPassword = confirmPassword };
                var response = await _httpClient.PostAsJsonAsync("api/Auth/register/fornecedor", payload);

                if (response.IsSuccessStatusCode)
                {
                    return new AuthResult { Success = true, Message = "Registo submetido! Aguarde aprovação." };
                }

                var error = await response.Content.ReadAsStringAsync();
                return new AuthResult { Success = false, Message = "Erro no registo de fornecedor." };
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

    public class UploadResult
    {
        public string? ImageUrl { get; set; }
        public string? Url { get; set; }
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

    #endregion
}