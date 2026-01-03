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
        private readonly UserService _userService;

        public CollectionApiService(HttpClient httpClient, UserService userService)
        {
            _httpClient = httpClient;
            _userService = userService;
        }

        private void SetAuthorizationHeader()
        {
            if (_userService.IsLoggedIn && _userService.CurrentUser != null && !string.IsNullOrEmpty(_userService.CurrentUser.Token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _userService.CurrentUser.Token);
            }
            else
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }

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
            catch
            {
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
            catch
            {
                return new List<Produto>();
            }
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            try
            {
                SetAuthorizationHeader();
                var response = await _httpClient.DeleteAsync($"api/Produtos/{id}");
                return response.IsSuccessStatusCode;
            }
            catch
            {
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
            catch
            {
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
            catch
            {
                return new List<Produto>();
            }
        }

        public async Task<List<Produto>> GetMyProductsAsync()
        {
            try
            {
                SetAuthorizationHeader();
                var products = await _httpClient.GetFromJsonAsync<List<Produto>>("api/Produtos/meus") ?? new();
                foreach (var p in products) FixImageUrl(p);
                return products;
            }
            catch
            {
                return new List<Produto>();
            }
        }

        public async Task<string?> CreateProductAsync(ProdutoCreateDto produto)
        {
            try
            {
                SetAuthorizationHeader();

                var response = await _httpClient.PostAsJsonAsync("api/Produtos", produto);
                if (response.IsSuccessStatusCode)
                {
                    return null;
                }

                var errorDetail = await response.Content.ReadAsStringAsync();
                return $"Erro na API: {response.StatusCode} - {errorDetail}";
            }
            catch (Exception ex)
            {
                return $"Erro de conexão: {ex.Message}";
            }
        }

        public async Task<string?> UploadImageAsync(IBrowserFile file)
        {
            try
            {
                SetAuthorizationHeader();

                long maxFileSize = 1024 * 1024 * 5;

                using var content = new MultipartFormDataContent();
                using var fileContent = new StreamContent(file.OpenReadStream(maxFileSize));
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

                content.Add(fileContent, "file", file.Name);

                var response = await _httpClient.PostAsync("api/Upload/produto", content);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<UploadResult>();

                    string? partialUrl = result?.ImageUrl ?? result?.Url;
                    if (!string.IsNullOrEmpty(partialUrl))
                    {
                        return $"{GetBaseUrl().TrimEnd('/')}{partialUrl}";
                    }
                }
                return null;
            }
            catch
            {
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
                    return new AuthResult { Success = true, Token = result?.Token, Message = "Login efetuado com sucesso" };
                }
                return new AuthResult { Success = false, Message = "Credenciais inválidas" };
            }
            catch
            {
                return new AuthResult { Success = false, Message = "Erro de conexão" };
            }
        }

        public async Task<AuthResult> RegisterUserAsync(RegisterUserDto model)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/Auth/register", model);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<JsonElement>();
                    string msg = result.TryGetProperty("message", out var m) ? m.ToString() : "Registo efetuado com sucesso";
                    return new AuthResult { Success = true, Message = msg };
                }

                var error = await response.Content.ReadAsStringAsync();
                return new AuthResult { Success = false, Message = $"Erro: {error}" };
            }
            catch
            {
                return new AuthResult { Success = false, Message = "Erro de conexão com o servidor" };
            }
        }

        public async Task<UserProfileModel?> GetProfileAsync()
        {
            try
            {
                SetAuthorizationHeader();
                return await _httpClient.GetFromJsonAsync<UserProfileModel>("api/Auth/me");
            }
            catch { return null; }
        }

        public async Task<bool> UpdateProfileAsync(UserProfileModel profile)
        {
            try
            {
                SetAuthorizationHeader();
                var response = await _httpClient.PutAsJsonAsync("api/Auth/profile", profile);
                return response.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        public async Task<AuthResult> ChangePasswordAsync(string current, string newPass, string confirm)
        {
            try
            {
                SetAuthorizationHeader();
                var payload = new { CurrentPassword = current, NewPassword = newPass, ConfirmNewPassword = confirm };
                var response = await _httpClient.PostAsJsonAsync("api/Auth/change-password", payload);

                if (response.IsSuccessStatusCode)
                    return new AuthResult { Success = true, Message = "Password alterada!" };

                var error = await response.Content.ReadAsStringAsync();
                return new AuthResult { Success = false, Message = "Erro: " + error };
            }
            catch (Exception ex)
            {
                return new AuthResult { Success = false, Message = ex.Message };
            }
        }

        #endregion

        #region Orders

        public async Task<OrderResult> CreateOrderAsync(EncomendaCreateDto order)
        {
            try
            {
                SetAuthorizationHeader();

                var response = await _httpClient.PostAsJsonAsync("api/Encomendas", order);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<JsonElement>();
                    string msg = result.TryGetProperty("message", out var m) ? m.ToString() : "Encomenda criada com sucesso!";
                    return new OrderResult { Success = true, Message = msg };
                }

                var errorContent = await response.Content.ReadAsStringAsync();

                try
                {
                    var errorJson = JsonSerializer.Deserialize<JsonElement>(errorContent);
                    if (errorJson.TryGetProperty("message", out var errorMsg))
                    {
                        return new OrderResult { Success = false, Message = errorMsg.GetString() ?? "Erro ao criar encomenda" };
                    }
                }
                catch { }

                return new OrderResult { Success = false, Message = "Erro ao criar encomenda" };
            }
            catch
            {
                return new OrderResult { Success = false, Message = "Erro de conexão" };
            }
        }

        public async Task<List<Encomenda>> GetMyOrdersAsync()
        {
            try
            {
                SetAuthorizationHeader();
                return await _httpClient.GetFromJsonAsync<List<Encomenda>>("api/Encomendas/minhas") ?? new();
            }
            catch
            {
                return new List<Encomenda>();
            }
        }

        public async Task<Encomenda?> GetOrderByIdAsync(int id)
        {
            try
            {
                SetAuthorizationHeader();
                return await _httpClient.GetFromJsonAsync<Encomenda>($"api/Encomendas/{id}");
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<ModoEntrega>> GetDeliveryModesAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<ModoEntrega>>("api/ModosEntrega") ?? new();
            }
            catch
            {
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

    public class EncomendaCreateDto
    {
        public string MoradaEnvio { get; set; } = string.Empty;
        public int ModoEntregaId { get; set; }
        public List<CarrinhoItemDto> Itens { get; set; } = new();
    }

    public class CarrinhoItemDto
    {
        public int ProdutoId { get; set; }
        public int Quantidade { get; set; }
    }

    public class RegisterUserDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
        public bool Fornecedor { get; set; } = false;
        public string? NomeEmpresa { get; set; }
        public string? NIF { get; set; }
        public string? TelefoneEmpresa { get; set; }
        public string? MoradaEmpresa { get; set; }
    }

    public class UserProfileModel
    {
        public string Email { get; set; } = string.Empty;
        public string NomeCompleto { get; set; } = string.Empty;
        public bool IsFornecedor { get; set; }
        public string? NomeEmpresa { get; set; }
        public string? NIF { get; set; }
        public string? TelefoneEmpresa { get; set; }
        public string? MoradaEmpresa { get; set; }
    }

    #endregion
}