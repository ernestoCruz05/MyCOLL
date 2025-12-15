using System.ComponentModel.DataAnnotations.Schema;

namespace MyCOLL.UIComponents.Models
{
    public class Categoria
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public string? ImagemBase64 { get; set; }
        public string? ImagemTipo { get; set; }
        public bool Ativa { get; set; } = true;

        [NotMapped]
        public string? ImagemDataUrl =>
            !string.IsNullOrEmpty(ImagemBase64) && !string.IsNullOrEmpty(ImagemTipo)
                ? $"data:{ImagemTipo};base64,{ImagemBase64}"
                : null;
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
        public string? ImagemBase64 { get; set; }
        public string? ImagemTipo { get; set; }

        [NotMapped]
        public string? ImagemDataUrl =>
            !string.IsNullOrEmpty(ImagemBase64) && !string.IsNullOrEmpty(ImagemTipo)
                ? $"data:{ImagemTipo};base64,{ImagemBase64}"
                : null;
    }

    public class Subcategoria
    {
        public int Id { get; set; }
        public int CategoriaId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? ImagemBase64 { get; set; }
        public string? ImagemTipo { get; set; }
    }
}