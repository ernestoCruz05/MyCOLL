using Microsoft.EntityFrameworkCore;
using MyCOLL.Data;
using MyCOLL.Entities;

// Ler ProdutoService.cs


namespace MyCOLL.Services
{
    public class CategoriaService
    {
        private readonly ApplicationDbContext _context;

        public CategoriaService(ApplicationDbContext context)
        {
            _context = context;
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
        }

        public async Task UpdateAsync(Categoria categoria)
        {
            _context.Categorias.Update(categoria);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var cat = await _context.Categorias.FindAsync(id);
            if (cat != null)
            {
                _context.Categorias.Remove(cat);
                await _context.SaveChangesAsync();
            }
        }
    }
}
