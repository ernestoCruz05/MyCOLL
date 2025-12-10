namespace MyCOLL.UIComponents.Models
{
    public class Categoria
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
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
}