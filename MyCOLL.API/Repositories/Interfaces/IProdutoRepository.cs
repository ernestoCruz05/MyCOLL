using MyCOLL.API.Entities;

namespace MyCOLL.API.Repositories.Interfaces
{
    public interface IProdutoRepository
    {
        Task<IEnumerable<Produto>> GetAllAsync();
        Task<IEnumerable<Produto>> GetByCategoriaAsync(int categoriaId);
        Task<Produto?> GetByIdAsync(int id);
    }
}