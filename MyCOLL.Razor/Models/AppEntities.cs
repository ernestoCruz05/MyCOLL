using System.ComponentModel.DataAnnotations.Schema;

namespace MyCOLL.UIComponents.Models
{
    public class Categoria
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public string? ImagemUrl { get; set; }
        public bool Ativa { get; set; } = true;
    }

    public class Produto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public decimal Preco { get; set; }
        public int Stock { get; set; }
        public bool Ativo { get; set; }
        public int CategoriaId { get; set; }
        public string? ImagemUrl { get; set; }
    }

    public class Subcategoria
    {
        public int Id { get; set; }
        public int CategoriaId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? ImagemUrl { get; set; }
    }
}