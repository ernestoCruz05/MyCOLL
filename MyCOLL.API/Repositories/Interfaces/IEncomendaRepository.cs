using MyCOLL.API.Entities;

namespace MyCOLL.API.Repositories.Interfaces
{
    public interface IEncomendaRepository
    {
        Task<IEnumerable<Encomenda>> GetAllAsync();
        Task<IEnumerable<Encomenda>> GetByClienteIdAsync(string clienteId);
        Task<Encomenda?> GetByIdAsync(int id);
        Task<Encomenda> CreateAsync(Encomenda encomenda);
        Task UpdateEstadoAsync(int id, EstadoEncomenda novoEstado);
        
        /// <summary>
        /// Obtém o total de unidades vendidas de um produto (soma das quantidades em encomendas não canceladas)
        /// </summary>
        Task<int> GetUnidadesVendidasPorProdutoAsync(int produtoId);
    }
}