using System.ComponentModel.DataAnnotations;

namespace MyCOLL.API.DTOs
{
    public class EncomendaCreateDto
    {
        [Required]
        public string ClienteId { get; set; } = string.Empty;

        [Required]
        public string MoradaEnvio { get; set; } = string.Empty;

        [Required]
        public int ModoEntregaId { get; set; }

        [Required]
        public List<CarrinhoItemDto> Itens { get; set; } = new List<CarrinhoItemDto>();
    }

    public class CarrinhoItemDto
    {
        [Required]
        public int ProdutoId { get; set; }

        [Range(1, 100)]
        public int Quantidade { get; set; }
    }
}