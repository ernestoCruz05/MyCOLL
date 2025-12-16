using MyCOLL.Data;
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

        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecoBase { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MargemLucro { get; set; } // 0 -> 100% 

        [Column(TypeName = "decimal(18,2)")]
        public decimal Preco { get; set; }

        public int Stock { get; set; }

        public bool Ativo { get; set; } = true;

        public int CategoriaId { get; set; }
        public Categoria? Categoria { get; set; }

        public int ModoEntregaId { get; set; }
        public ModoEntrega? ModoEntrega { get; set; }

        public string? FornecedorId { get; set; }

        [ForeignKey("FornecedorId")]
        public ApplicationUser? Fornecedor { get; set; }

        [StringLength(300)]
        public string? ImagemUrl { get; set; }

        public DateTime DataCriacao { get; set; } = DateTime.Now;
        public DateTime? DataAtualizacao { get; set; }

        public DateTime DataAdicao { get; set; } = DateTime.Now;
    }
}
