using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyCOLL.Entities
{
    public class ModoEntrega
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Nome { get; set; } = string.Empty;

        [StringLength(250)]
        public string? Descricao { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal CustoBase { get; set; }

        public int PrazoEstimadoDias { get; set; }

        public DateTime DataCriacao { get; set; } = DateTime.Now;
        public DateTime? DataAtualizacao { get; set; }

        public bool Ativo { get; set; } = true;
        public ICollection<Produto> Produtos { get; set; } = new List<Produto>();
    }
}
