using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyCOLL.Data;

namespace MyCOLL.Entities
{
    public enum EstadoEncomenda
    {
        Pendente,
        Paga,
        Expedida,
        Entregue,
        Cancelada
    }

    public class Encomenda
    {
        public int Id { get; set; }

        // Quem comprou
        [Required]
        public string ClienteId { get; set; } = string.Empty;
        [ForeignKey("ClienteId")]
        public ApplicationUser? Cliente { get; set; }

        public DateTime DataEncomenda { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(10,2)")]
        public decimal Total { get; set; }

        public EstadoEncomenda Estado { get; set; } = EstadoEncomenda.Pendente;

        // Dados de Envio (Snapshot no momento da compra)
        public string MoradaEnvio { get; set; } = string.Empty;
        public string MetodoEntregaNome { get; set; } = string.Empty;

        [Column(TypeName = "decimal(10,2)")]
        public decimal CustoEntrega { get; set; }

        // Lista de itens
        public ICollection<DetalheEncomenda> Itens { get; set; } = new List<DetalheEncomenda>();
    }
}