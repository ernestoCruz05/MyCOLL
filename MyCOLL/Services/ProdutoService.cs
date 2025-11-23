using Microsoft.EntityFrameworkCore;
using MyCOLL.Data;
using MyCOLL.Entities;

namespace MyCOLL.Services
{
    public class ProdutoService
    {
        private readonly ApplicationDbContext _context;
        private readonly LogService _log; // Adicionado LogService

        public ProdutoService(ApplicationDbContext context, LogService log)
        {
            _context = context;
            _log = log;
        }

        public async Task<List<Produto>> GetAllAsync() =>
            await _context.Produtos
                .Include(p => p.Categoria)
                .Include(p => p.ModoEntrega)
                .OrderByDescending(p => p.Id)
                .ToListAsync();

        public async Task<Produto?> GetByIdAsync(int id) =>
            await _context.Produtos
                .Include(p => p.Categoria)
                .Include(p => p.ModoEntrega)
                .FirstOrDefaultAsync(p => p.Id == id);

        public async Task AddAsync(Produto produto)
        {
            produto.DataCriacao = DateTime.Now;
            produto.DataAtualizacao = null;

            _context.Produtos.Add(produto);
            await _context.SaveChangesAsync();

            await _log.AddAsync("Produto", "Criado", produto.Nome);
        }

        public async Task UpdateAsync(Produto produto)
        {
            produto.DataAtualizacao = DateTime.Now;

            _context.Produtos.Update(produto);
            await _context.SaveChangesAsync();

            await _log.AddAsync("Produto", "Atualizado", produto.Nome);
        }

        public async Task DeleteAsync(int id)
        {
            var prod = await _context.Produtos.FindAsync(id);
            if (prod != null)
            {
                _context.Produtos.Remove(prod);
                await _context.SaveChangesAsync();

                await _log.AddAsync("Produto", "Eliminado", prod.Nome);
            }
        }
    }
}