using Microsoft.EntityFrameworkCore;
using MyCOLL.Data;
using MyCOLL.Entities;

// Ler ProdutoService.cs


namespace MyCOLL.Services
{
    public class CategoriaService
    {
        private readonly ApplicationDbContext _context;
        private readonly LogService _log;
        public CategoriaService(ApplicationDbContext context, LogService log)
        {
            _context = context;
            _log = log;
        }

        public async Task<List<Categoria>> GetAllAsync() =>
            await _context.Categorias
                .Include(c => c.Produtos)
                .ToListAsync();

        public async Task<Categoria?> GetByIdAsync(int id) =>
            await _context.Categorias
                .Include(c => c.Produtos)
                .FirstOrDefaultAsync(c => c.Id == id);

        public async Task AddAsync(Categoria categoria)
        {
            _context.Categorias.Add(categoria);
            await _context.SaveChangesAsync();

            await _log.AddAsync("Categoria", "Criada", categoria.Nome);
        }

        public async Task UpdateAsync(Categoria categoria)
        {
            categoria.DataAtualizacao = DateTime.Now;
            _context.Categorias.Update(categoria);
            await _context.SaveChangesAsync();

            await _log.AddAsync("Categoria", "Atualizada", categoria.Nome);
        }

        public async Task DeleteAsync(int id)
        {
            var cat = await _context.Categorias.FindAsync(id);
            if (cat != null)
            {
                _context.Categorias.Remove(cat);
                await _context.SaveChangesAsync();

                await _log.AddAsync("Categoria", "Eliminada", cat.Nome);
            }
        }
    }
}
