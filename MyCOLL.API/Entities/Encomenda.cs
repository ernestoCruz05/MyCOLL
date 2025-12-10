using MyCOLL.API.Data; // Para aceder a ApplicationUser
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyCOLL.API.Entities
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

        [Required]
        public string ClienteId { get; set; } = string.Empty;

        [ForeignKey("ClienteId")]
        public ApplicationUser? Cliente { get; set; }

        public DateTime DataEncomenda { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(10,2)")]
        public decimal Total { get; set; }

        public EstadoEncomenda Estado { get; set; } = EstadoEncomenda.Pendente;

        public string MoradaEnvio { get; set; } = string.Empty;
        public string MetodoEntregaNome { get; set; } = string.Empty;

        [Column(TypeName = "decimal(10,2)")]
        public decimal CustoEntrega { get; set; }

        public ICollection<DetalheEncomenda> Itens { get; set; } = new List<DetalheEncomenda>();
    }
}