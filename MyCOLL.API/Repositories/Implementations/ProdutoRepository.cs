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
                .OrderByDescending(p => p.DataCriacao)
                .ToListAsync();
        }

        public async Task<IEnumerable<Produto>> GetByCategoriaAsync(int categoriaId)
        {
            return await _context.Produtos
                .Include(p => p.Categoria)
                .Include(p => p.ModoEntrega)
                .Where(p => p.Ativo && p.CategoriaId == categoriaId)
                .OrderByDescending(p => p.DataCriacao)
                .ToListAsync();
        }

        public async Task<Produto?> GetByIdAsync(int id)
        {
            return await _context.Produtos
                .Include(p => p.Categoria)
                .Include(p => p.ModoEntrega)
                .FirstOrDefaultAsync(p => p.Id == id && p.Ativo);
        }

        public async Task<IEnumerable<Produto>> SearchAsync(string searchTerm)
        {
            var term = searchTerm.ToLower();
            return await _context.Produtos
                .Include(p => p.Categoria)
                .Include(p => p.ModoEntrega)
                .Where(p => p.Ativo &&
                    (p.Nome.ToLower().Contains(term) ||
                     (p.Descricao != null && p.Descricao.ToLower().Contains(term)) ||
                     (p.Categoria != null && p.Categoria.Nome.ToLower().Contains(term))))
                .OrderByDescending(p => p.DataCriacao)
                .ToListAsync();
        }

        public async Task<Produto?> GetRandomAsync()
        {
            var count = await _context.Produtos.CountAsync(p => p.Ativo);
            if (count == 0) return null;

            var skip = new Random().Next(0, count);
            return await _context.Produtos
                .Include(p => p.Categoria)
                .Include(p => p.ModoEntrega)
                .Where(p => p.Ativo)
                .Skip(skip)
                .FirstOrDefaultAsync();
        }

        // Novos métodos CRUD
        public async Task<IEnumerable<Produto>> GetByFornecedorIdAsync(string fornecedorId)
        {
            return await _context.Produtos
                .Include(p => p.Categoria)
                .Include(p => p.ModoEntrega)
                .Where(p => p.FornecedorId == fornecedorId)
                .OrderByDescending(p => p.DataCriacao)
                .ToListAsync();
        }

        public async Task<Produto> CreateAsync(Produto produto)
        {
            _context.Produtos.Add(produto);
            await _context.SaveChangesAsync();
            return produto;
        }

        public async Task UpdateAsync(Produto produto)
        {
            _context.Produtos.Update(produto);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var produto = await _context.Produtos.FindAsync(id);
            if (produto != null)
            {
                _context.Produtos.Remove(produto);
                await _context.SaveChangesAsync();
            }
        }
    }
}