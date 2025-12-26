using Microsoft.AspNetCore.Identity;

namespace MyCOLL.API.Data
{
    public class ApplicationUser : IdentityUser
    {
        public string NomeCompleto { get; set; } = string.Empty;
        public bool IsFornecedor { get; set; } = false;

        public string? NomeEmpresa { get; set; }
        public string? NIF { get; set; }
        public string? TelefoneEmpresa { get; set; }
        public string? MoradaEmpresa { get; set; }
    }
}
