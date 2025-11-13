using Microsoft.EntityFrameworkCore;
using MyCOLL.Data;
using MyCOLL.Entities;


// Services servem para o UI interagir com a DB sem ter acesso direto
// | UI | <-> | Services | <-> |DB|

namespace MyCOLL.Services
{
    public class ProdutoService
    {
        private readonly ApplicationDbContext _context;


        // Blah blah blah!!!
        // Injetar as dependencias do ASP.NET
        public ProdutoService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Obter a lista das entradas da DB
        public async Task<List<Produto>> GetAllAsync() =>
            await _context.Produtos
                .Include(p => p.Categoria)
                .Include(p => p.ModoEntrega)
                .ToListAsync();

        // Procura uma entrada na DB pelo ID, o ? indica que pode ser null
        public async Task<Produto?> GetByIdAsync(int id) =>
            await _context.Produtos
                .Include(p => p.Categoria)
                .Include(p => p.ModoEntrega)
                .FirstOrDefaultAsync(p => p.Id == id);

        // Adiciona uma entrada a DB
        public async Task AddAsync(Produto produto)
        {
            produto.DataCriacao = DateTime.Now;
            produto.DataAtualizacao = null;
            _context.Produtos.Add(produto);
            await _context.SaveChangesAsync();
        }

        // Atualiza as entradas da DB 
        public async Task UpdateAsync(Produto produto)
        {
            produto.DataAtualizacao = DateTime.Now;
            _context.Produtos.Update(produto);
            await _context.SaveChangesAsync();
        }

        // Remove uma entrada da DB
        public async Task DeleteAsync(int id)
        {
            var cat = await _context.Produtos.FindAsync(id);
            if (cat != null)
            {
                _context.Produtos.Remove(cat);
                await _context.SaveChangesAsync();
            }
        }
    }
}
