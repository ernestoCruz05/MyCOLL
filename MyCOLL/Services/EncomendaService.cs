using Microsoft.EntityFrameworkCore;
using MyCOLL.Data;
using MyCOLL.Entities;

namespace MyCOLL.Services
{
    public class EncomendaService
    {
        private readonly ApplicationDbContext _context;
        private readonly LogService _log;

        public EncomendaService(ApplicationDbContext context, LogService log)
        {
            _context = context;
            _log = log;
        }

        public async Task<List<Encomenda>> GetAllAsync()
        {
            return await _context.Encomendas
                .Include(e => e.Cliente)
                .Include(e => e.Itens)
                    .ThenInclude(i => i.Produto)
                .OrderByDescending(e => e.DataEncomenda)
                .ToListAsync();
        }

        public async Task<Encomenda?> GetByIdAsync(int id)
        {
            return await _context.Encomendas
                .Include(e => e.Cliente)
                .Include(e => e.Itens)
                    .ThenInclude(i => i.Produto)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task UpdateEstadoAsync(int id, EstadoEncomenda novoEstado)
        {
            var encomenda = await _context.Encomendas
                .Include(e => e.Itens)
                .ThenInclude(i => i.Produto)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (encomenda != null)
            {
                var estadoAntigo = encomenda.Estado;

                if (estadoAntigo == novoEstado) return;

                encomenda.Estado = novoEstado;

                if (estadoAntigo != EstadoEncomenda.Expedida && novoEstado == EstadoEncomenda.Expedida)
                {
                    foreach (var item in encomenda.Itens)
                    {
                        if (item.Produto != null)
                        {
                            item.Produto.Stock -= item.Quantidade;
                            if (item.Produto.Stock < 0) item.Produto.Stock = 0;
                        }
                    }
                }

                if (estadoAntigo == EstadoEncomenda.Expedida && novoEstado == EstadoEncomenda.Cancelada)
                {
                    foreach (var item in encomenda.Itens)
                    {
                        if (item.Produto != null)
                        {
                            item.Produto.Stock += item.Quantidade;
                        }
                    }
                }

                await _context.SaveChangesAsync();
                await _log.AddAsync("Encomenda", $"Estado mudou: {estadoAntigo} -> {novoEstado}", $"Encomenda #{id}");
            }
        }
    }
}