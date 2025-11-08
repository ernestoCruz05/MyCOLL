using Microsoft.EntityFrameworkCore;
using MyCOLL.Data;
using MyCOLL.Entities;

// Ler ProdutoService.cs


namespace MyCOLL.Services
{
    public class ModoEntregaService
    {
        private readonly ApplicationDbContext _context;

        public ModoEntregaService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ModoEntrega>> GetAllAsync() =>
            await _context.ModosEntrega
                .Include(m => m.Produtos)
                .ToListAsync();

        public async Task<ModoEntrega?> GetByIdAsync(int id) =>
            await _context.ModosEntrega
                .Include(m => m.Produtos)
                .FirstOrDefaultAsync(m => m.Id == id);

        public async Task AddAsync(ModoEntrega modo)
        {
            _context.ModosEntrega.Add(modo);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ModoEntrega modo)
        {
            _context.ModosEntrega.Update(modo);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var cat = await _context.ModosEntrega.FindAsync(id);
            if (cat != null)
            {
                _context.ModosEntrega.Remove(cat);
                await _context.SaveChangesAsync();
            }
        }
    }
}
