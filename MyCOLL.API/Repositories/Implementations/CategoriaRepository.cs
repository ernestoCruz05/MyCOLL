using Microsoft.EntityFrameworkCore;
using MyCOLL.API.Data;
using MyCOLL.API.Entities;
using MyCOLL.API.Repositories.Interfaces;

namespace MyCOLL.API.Repositories.Implementations
{
    public class CategoriaRepository : ICategoriaRepository
    {
        private readonly AppDbContext _context;

        public CategoriaRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Categoria>> GetAllAsync()
        {
            return await _context.Categorias.ToListAsync();
        }

        public async Task<Categoria?> GetByIdAsync(int id)
        {
            return await _context.Categorias.FindAsync(id);
        }
    }
}