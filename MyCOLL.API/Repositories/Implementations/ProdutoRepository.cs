using Microsoft.EntityFrameworkCore;
using MyCOLL.API.Data;
using MyCOLL.API.Entities;
using MyCOLL.API.Repositories.Interfaces;

namespace MyCOLL.API.Repositories.Implementations
{
    public class ProdutoRepository : IProdutoRepository
    {
        private readonly AppDbContext _context;

        public ProdutoRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Produto>> GetAllAsync()
        {
            return await _context.Produtos
                .Include(p => p.Categoria)
                .Include(p => p.ModoEntrega)
                .Where(p => p.Ativo)
                .ToListAsync();
        }

        public async Task<IEnumerable<Produto>> GetByCategoriaAsync(int categoriaId)
        {
            return await _context.Produtos
                .Include(p => p.Categoria)
                .Include(p => p.ModoEntrega)
                .Where(p => p.Ativo && p.CategoriaId == categoriaId)
                .ToListAsync();
        }

        public async Task<Produto?> GetByIdAsync(int id)
        {
            return await _context.Produtos
                .Include(p => p.Categoria)
                .Include(p => p.ModoEntrega)
                .FirstOrDefaultAsync(p => p.Id == id && p.Ativo);
        }
    }
}