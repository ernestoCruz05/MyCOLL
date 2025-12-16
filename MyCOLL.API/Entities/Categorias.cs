using System.ComponentModel.DataAnnotations;

namespace MyCOLL.API.Entities
{
    public class Categoria
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Nome { get; set; } = string.Empty;

        [StringLength(255)]
        public string? Descricao { get; set; }


        [StringLength(500)]
        public string? ImagemUrl { get; set; }

        public bool Ativa { get; set; } = true;

        public DateTime? DataAtualizacao { get; set; }

        public ICollection<Produto> Produtos { get; set; } = new List<Produto>();
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    }
}
