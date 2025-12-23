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


        [Range(0, 1000, ErrorMessage = "Margem de lucro deve ser entre 0 e 1000%")]
        public decimal MargemLucro { get; set; } = 0; 
        // deve ser alterada atraves de edicao por parte do gestor

        [Required(ErrorMessage = "Stock é obrigatório")]
        [Range(0, 99999, ErrorMessage = "Stock deve ser entre 0 e 99999")]
        public int Stock { get; set; }

        [Required(ErrorMessage = "Categoria é obrigatória")]
        public int CategoriaId { get; set; }

        [Required(ErrorMessage = "Modo de entrega é obrigatório")]
        public int ModoEntregaId { get; set; }

        public string? ImagemUrl { get; set; }
    }

    public class ProdutoUpdateDto : ProdutoCreateDto
    {
        // Podemos herdar do Create para evitar duplicação, 
        // ou manter separado se preferir, mas remova o [Required] da MargemLucro aqui também.
    }
}