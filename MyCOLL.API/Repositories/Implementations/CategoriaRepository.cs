using MyCOLL.API.Data;
using MyCOLL.API.Entities;
using MyCOLL.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

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
            => await _context.Categorias.ToListAsync();

        public async Task<Categoria?> GetByIdAsync(int id)
            => await _context.Categorias.FindAsync(id);
    }
}
