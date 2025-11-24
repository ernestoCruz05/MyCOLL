using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyCOLL.API.Entities
{
    public class ModoEntrega
    {
        public int Id { get; set; }
        [Required, StringLength(100)]
        public string Nome { get; set; } = string.Empty;

        [Column(TypeName = "decimal(10,2)")]
        public decimal CustoBase { get; set; }

        public int PrazoEstimadoDias { get; set; }
        public bool Ativo { get; set; } = true;
    }
}