using System.ComponentModel.DataAnnotations.Schema;

namespace MyCOLL.Entities
{
    public class DetalheEncomenda
    {
        public int Id { get; set; }

        public int EncomendaId { get; set; }
        public Encomenda? Encomenda { get; set; }

        public int ProdutoId { get; set; }
        public Produto? Produto { get; set; }

        public int Quantidade { get; set; }

        // Preço unitário no momento da compra
        [Column(TypeName = "decimal(10,2)")]
        public decimal PrecoUnitario { get; set; }
    }
}