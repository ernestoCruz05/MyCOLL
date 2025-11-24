using Microsoft.EntityFrameworkCore;
using MyCOLL.API.Data;
using MyCOLL.API.Entities;
using MyCOLL.API.Repositories.Interfaces;

namespace MyCOLL.API.Repositories.Implementations
{
    public class ModoEntregaRepository : IModoEntregaRepository
    {
        private readonly AppDbContext _context;

        public ModoEntregaRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ModoEntrega>> GetAllAsync()
        {
            return await _context.ModosEntrega
                .Where(m => m.Ativo) // Only return active modes to the public app
                .ToListAsync();
        }

        public async Task<ModoEntrega?> GetByIdAsync(int id)
        {
            return await _context.ModosEntrega
                .FirstOrDefaultAsync(m => m.Id == id && m.Ativo);
        }
    }
}