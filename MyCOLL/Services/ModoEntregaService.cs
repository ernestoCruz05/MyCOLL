using Microsoft.EntityFrameworkCore;
using MyCOLL.Data;
using MyCOLL.Entities;

namespace MyCOLL.Services
{
    public class ModoEntregaService
    {
        private readonly ApplicationDbContext _context;
        private readonly LogService _log; // Injeção do Log

        public ModoEntregaService(ApplicationDbContext context, LogService log)
        {
            _context = context;
            _log = log;
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
            modo.DataCriacao = DateTime.Now;
            modo.DataAtualizacao = null;

            _context.ModosEntrega.Add(modo);
            await _context.SaveChangesAsync();

            await _log.AddAsync("Modo Entrega", "Criado", modo.Nome);
        }

        public async Task UpdateAsync(ModoEntrega modo)
        {
            modo.DataAtualizacao = DateTime.Now;

            _context.ModosEntrega.Update(modo);
            await _context.SaveChangesAsync();

            await _log.AddAsync("Modo Entrega", "Atualizado", modo.Nome);
        }

        public async Task DeleteAsync(int id)
        {
            // Alterado para Include para verificar produtos antes de apagar
            var modo = await _context.ModosEntrega
                .Include(m => m.Produtos)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (modo != null)
            {
                // Impede apagar se já estiver em uso
                if (modo.Produtos.Any())
                {
                    throw new InvalidOperationException($"Não é possível apagar '{modo.Nome}' porque existem produtos associados a este modo de entrega.");
                }

                _context.ModosEntrega.Remove(modo);
                await _context.SaveChangesAsync();

                await _log.AddAsync("Modo Entrega", "Eliminado", modo.Nome);
            }
        }
    }
}