using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyCOLL.Entities
{
    public class Produto
    {
        public int Id { get; set; }

        [Required, StringLength(120)]
        public string Nome { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Descricao { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Preco { get; set; }

        public int Stock { get; set; }

        public bool Ativo { get; set; } = true;

        public int CategoriaId { get; set; }
        public Categoria? Categoria { get; set; }

        public int ModoEntregaId { get; set; }
        public ModoEntrega? ModoEntrega { get; set; }

        [StringLength(300)]
        public string? ImagemUrl { get; set; }

        public DateTime DataCriacao { get; set; } = DateTime.Now;
        public DateTime? DataAtualizacao { get; set; }

        public DateTime DataAdicao { get; set; } = DateTime.Now;
    }
}
