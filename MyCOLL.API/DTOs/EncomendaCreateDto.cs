using System.ComponentModel.DataAnnotations;

namespace MyCOLL.API.DTOs
{
    public class EncomendaCreateDto
    {
        // ClienteId REMOVIDO - será obtido do token JWT por segurança!

        [Required(ErrorMessage = "Morada de envio é obrigatória")]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "Morada deve ter entre 5 e 200 caracteres")]
        public string MoradaEnvio { get; set; } = string.Empty;

        [Required(ErrorMessage = "Modo de entrega é obrigatório")]
        public int ModoEntregaId { get; set; }

        [Required(ErrorMessage = "A encomenda deve ter itens")]
        [MinLength(1, ErrorMessage = "A encomenda deve ter pelo menos 1 item")]
        public List<CarrinhoItemDto> Itens { get; set; } = new();
    }

    public class CarrinhoItemDto
    {
        [Required]
        public int ProdutoId { get; set; }

        [Range(1, 100, ErrorMessage = "Quantidade deve ser entre 1 e 100")]
        public int Quantidade { get; set; }
    }
}