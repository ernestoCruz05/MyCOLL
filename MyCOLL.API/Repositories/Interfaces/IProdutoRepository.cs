using MyCOLL.API.Entities;

namespace MyCOLL.API.Repositories.Interfaces
{
    public interface IProdutoRepository
    {
        // Endpoints públicos (já existem)
        Task<IEnumerable<Produto>> GetAllAsync();
        Task<IEnumerable<Produto>> GetByCategoriaAsync(int categoriaId);
        Task<Produto?> GetByIdAsync(int id);
        Task<IEnumerable<Produto>> SearchAsync(string searchTerm);
        Task<Produto?> GetRandomAsync();

        // Novos: incluem produtos inativos (para loja)
        Task<IEnumerable<Produto>> GetAllIncludingInactiveAsync();
        Task<IEnumerable<Produto>> GetByCategoriaIncludingInactiveAsync(int categoriaId);
        Task<Produto?> GetByIdIncludingInactiveAsync(int id);
        Task<IEnumerable<Produto>> SearchIncludingInactiveAsync(string searchTerm);

        // Novos: CRUD para Fornecedores/Admin
        Task<IEnumerable<Produto>> GetByFornecedorIdAsync(string fornecedorId);
        Task<Produto> CreateAsync(Produto produto);
        Task UpdateAsync(Produto produto);
        Task DeleteAsync(int id);
    }
}