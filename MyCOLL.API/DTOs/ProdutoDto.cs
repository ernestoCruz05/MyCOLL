using System.ComponentModel.DataAnnotations;

namespace MyCOLL.API.DTOs
{
    public class ProdutoCreateDto
    {
        [Required(ErrorMessage = "Nome é obrigatório")]
        [StringLength(120, MinimumLength = 3, ErrorMessage = "Nome deve ter entre 3 e 120 caracteres")]
        public string Nome { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Descrição não pode exceder 500 caracteres")]
        public string? Descricao { get; set; }

        [Required(ErrorMessage = "Preço base é obrigatório")]
        [Range(0.01, 999999, ErrorMessage = "Preço base deve ser maior que 0")]
        public decimal PrecoBase { get; set; }

        [Required(ErrorMessage = "Margem de lucro é obrigatória")]
        [Range(0, 1000, ErrorMessage = "Margem de lucro deve ser entre 0 e 1000%")]
        public decimal MargemLucro { get; set; }

        [Required(ErrorMessage = "Stock é obrigatório")]
        [Range(0, 99999, ErrorMessage = "Stock deve ser entre 0 e 99999")]
        public int Stock { get; set; }

        [Required(ErrorMessage = "Categoria é obrigatória")]
        public int CategoriaId { get; set; }

        [Required(ErrorMessage = "Modo de entrega é obrigatório")]
        public int ModoEntregaId { get; set; }

        public string? ImagemUrl { get; set; }
    }

    public class ProdutoUpdateDto
    {
        [Required(ErrorMessage = "Nome é obrigatório")]
        [StringLength(120, MinimumLength = 3)]
        public string Nome { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Descricao { get; set; }

        [Required]
        [Range(0.01, 999999)]
        public decimal PrecoBase { get; set; }

        [Required]
        [Range(0, 1000)]
        public decimal MargemLucro { get; set; }

        [Required]
        [Range(0, 99999)]
        public int Stock { get; set; }

        [Required]
        public int CategoriaId { get; set; }

        [Required]
        public int ModoEntregaId { get; set; }

        public string? ImagemUrl { get; set; }
    }
}
