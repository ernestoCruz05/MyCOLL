using Microsoft.EntityFrameworkCore;
using MyCOLL.Data;
using MyCOLL.Entities;

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
            await _context.Categorias.ToListAsync();
        public async Task<List<Categoria>> GetPrincipaisComSubAsync()
        {
            return await _context.Categorias
                .Where(c => c.CategoriaPaiId == null) 
                .Include(c => c.SubCategorias)        
                .OrderBy(c => c.Nome)
                .ToListAsync();
        }

        public async Task<List<Categoria>> GetPossiveisPaisAsync()
        {
            return await _context.Categorias
                .Where(c => c.CategoriaPaiId == null && c.Ativa) 
                .OrderBy(c => c.Nome)
                .ToListAsync();
        }
        // ------------------------------------------

        public async Task<Categoria?> GetByIdAsync(int id) =>
            await _context.Categorias
                .Include(c => c.SubCategorias) 
                .FirstOrDefaultAsync(c => c.Id == id);

        public async Task AddAsync(Categoria categoria)
        {
            if (categoria.CategoriaPaiId.HasValue)
            {
                categoria.SubCategorias = new List<Categoria>();
            }

            categoria.DataCriacao = DateTime.Now;
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
            var cat = await _context.Categorias
                .Include(c => c.Produtos)
                .Include(c => c.SubCategorias)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cat != null)
            {
                if (cat.Produtos.Any())
                    throw new InvalidOperationException($"Não é possível apagar '{cat.Nome}' pois tem produtos.");

                if (cat.SubCategorias.Any())
                    throw new InvalidOperationException($"Não é possível apagar '{cat.Nome}' pois tem subcategorias.");

                _context.Categorias.Remove(cat);
                await _context.SaveChangesAsync();
                await _log.AddAsync("Categoria", "Eliminada", cat.Nome);
            }
        }
    }
}