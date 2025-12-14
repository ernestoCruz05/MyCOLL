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

            if (encomenda == null)
                throw new InvalidOperationException("Encomenda não encontrada.");

            var estadoAntigo = encomenda.Estado;
            if (estadoAntigo == novoEstado) return;

            // RN03: Validar transição de estado
            if (!IsValidTransition(estadoAntigo, novoEstado))
                throw new InvalidOperationException(
                    $"Transição de '{estadoAntigo}' para '{novoEstado}' não é permitida.");

            // RN03: Validar stock ANTES de expedir
            if (novoEstado == EstadoEncomenda.Expedida)
            {
                foreach (var item in encomenda.Itens)
                {
                    if (item.Produto != null && item.Produto.Stock < item.Quantidade)
                    {
                        throw new InvalidOperationException(
                            $"Stock insuficiente para '{item.Produto.Nome}'. " +
                            $"Disponível: {item.Produto.Stock}, Necessário: {item.Quantidade}");
                    }
                }
            }

            encomenda.Estado = novoEstado;

            // RN03: Decrementar stock ao expedir
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

            // RN03: Repor stock se cancelar após expedir
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
            await _log.AddAsync("Encomenda", $"Estado: {estadoAntigo} → {novoEstado}", $"Encomenda #{id}");
        }

        /// <summary>
        /// RN03: Valida transições de estado (Pendente → Paga → Expedida → Entregue)
        /// </summary>
        private static bool IsValidTransition(EstadoEncomenda atual, EstadoEncomenda novo)
        {
            // Estados terminais
            if (atual == EstadoEncomenda.Entregue || atual == EstadoEncomenda.Cancelada)
                return false;

            return (atual, novo) switch
            {
                (EstadoEncomenda.Pendente, EstadoEncomenda.Paga) => true,
                (EstadoEncomenda.Pendente, EstadoEncomenda.Cancelada) => true,
                (EstadoEncomenda.Paga, EstadoEncomenda.Expedida) => true,
                (EstadoEncomenda.Paga, EstadoEncomenda.Cancelada) => true,
                (EstadoEncomenda.Expedida, EstadoEncomenda.Entregue) => true,
                (EstadoEncomenda.Expedida, EstadoEncomenda.Cancelada) => true,
                _ => false
            };
        }
    }
}