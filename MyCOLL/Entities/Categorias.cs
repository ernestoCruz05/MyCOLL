using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyCOLL.Entities
{
    public class Categoria
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Nome { get; set; } = string.Empty;

        [StringLength(255)]
        public string? Descricao { get; set; }

        [StringLength(300)]
        public string? ImagemUrl { get; set; }

        public bool Ativa { get; set; } = true;

        public DateTime DataCriacao { get; set; } = DateTime.Now;
        public DateTime? DataAtualizacao { get; set; }

        public int? CategoriaPaiId { get; set; }

        [ForeignKey("CategoriaPaiId")]
        public Categoria? CategoriaPai { get; set; }

        public ICollection<Categoria> SubCategorias { get; set; } = new List<Categoria>();

        public ICollection<Produto> Produtos { get; set; } = new List<Produto>();
    }
}
