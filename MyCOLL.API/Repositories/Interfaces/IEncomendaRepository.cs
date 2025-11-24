using MyCOLL.API.Entities;

namespace MyCOLL.API.Repositories.Interfaces
{
    public interface IEncomendaRepository
    {
        Task<Encomenda> CreateAsync(Encomenda encomenda);
    }
}