using System.ComponentModel.DataAnnotations;

namespace MyCOLL.API.DTOs
{
    public class UserProfileDto
    {
        public string NomeCompleto { get; set; } = string.Empty;
        public string? NomeEmpresa { get; set; }
        public string? NIF { get; set; }
        public string? TelefoneEmpresa { get; set; }
        public string? MoradaEmpresa { get; set; }
    }

    public class ChangePasswordDto
    {
        [Required]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string NewPassword { get; set; } = string.Empty;

        [Compare("NewPassword")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}