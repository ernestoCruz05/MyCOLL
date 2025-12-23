using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MyCOLL.UIComponents.Models
{
    public class Categoria
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public string? ImagemUrl { get; set; }
        public bool Ativa { get; set; }
        public int? CategoriaPaiId { get; set; }
        public List<Categoria> SubCategorias { get; set; } = new();
    }

    public class ModoEntrega
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public decimal Preco { get; set; }
        public string? Descricao { get; set; }
    }

    public class UserInfoModel
    {
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class Produto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Descricao { get; set; }

        public decimal PrecoBase { get; set; }
        public decimal MargemLucro { get; set; }

        public decimal Preco { get; set; }
        public int Stock { get; set; }
        public bool Ativo { get; set; }

        public int CategoriaId { get; set; }
        public Categoria? Categoria { get; set; }

        public int? ModoEntregaId { get; set; }
        public ModoEntrega? ModoEntrega { get; set; }

        public string? FornecedorId { get; set; }
        public UserInfoModel? Fornecedor { get; set; }

        public string? ImagemUrl { get; set; }
    }

    public class ProdutoCreateDto
    {
        [Required(ErrorMessage = "O Nome é obrigatório.")]
        public string Nome { get; set; } = string.Empty;

        public string Descricao { get; set; } = string.Empty;

        [Required(ErrorMessage = "O Preço é obrigatório.")]
        [Range(0.01, 999999, ErrorMessage = "O Preço deve ser superior a 0.")]
        public decimal PrecoBase { get; set; }

        [Required(ErrorMessage = "O Stock é obrigatório.")]
        [Range(0, 99999, ErrorMessage = "O Stock não pode ser negativo.")]
        public int Stock { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Selecione uma categoria.")]
        public int CategoriaId { get; set; }

        [Required(ErrorMessage = "O Modo de Entrega é obrigatório.")]
        public int? ModoEntregaId { get; set; }

        public string ImagemUrl { get; set; } = string.Empty;
    }
}