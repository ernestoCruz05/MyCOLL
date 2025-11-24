using MyCOLL.API.Entities;

namespace MyCOLL.API.Repositories.Interfaces
{
    public interface IModoEntregaRepository
    {
        Task<IEnumerable<ModoEntrega>> GetAllAsync();
        Task<ModoEntrega?> GetByIdAsync(int id);
    }
}