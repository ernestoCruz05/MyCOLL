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

<<<<<<< Updated upstream
        [StringLength(300)]
=======
        [StringLength(500)]
>>>>>>> Stashed changes
        public string? ImagemUrl { get; set; }

        public bool Ativa { get; set; } = true;

<<<<<<< Updated upstream
        public DateTime DataCriacao { get; set; } = DateTime.Now;
        public DateTime? DataAtualizacao { get; set; }

        public ICollection<Produto> Produtos { get; set; } = new List<Produto>();
=======
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
>>>>>>> Stashed changes
    }
}
