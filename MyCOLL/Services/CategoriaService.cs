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
            await _context.Categorias
                .Include(c => c.Produtos)
                .OrderBy(c => c.Nome)
                .ToListAsync();

        public async Task<Categoria?> GetByIdAsync(int id) =>
            await _context.Categorias
                .Include(c => c.Produtos)
                .FirstOrDefaultAsync(c => c.Id == id);

        public async Task AddAsync(Categoria categoria)
        {
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
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cat != null)
            {
                // RN02: Não eliminar se tiver produtos
                if (cat.Produtos != null && cat.Produtos.Any())
                    throw new InvalidOperationException(
                        $"Não é possível apagar '{cat.Nome}' porque existem {cat.Produtos.Count} produtos associados.");

                _context.Categorias.Remove(cat); // BUG CORRIGIDO: faltava esta linha!
                await _context.SaveChangesAsync();
                await _log.AddAsync("Categoria", "Eliminada", cat.Nome);
            }
        }
    }
}
