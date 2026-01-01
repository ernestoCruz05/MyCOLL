using Microsoft.EntityFrameworkCore;
using MyCOLL.API.Data;
using MyCOLL.API.Entities;
using MyCOLL.API.Repositories.Interfaces;

namespace MyCOLL.API.Repositories.Implementations
{
    public class EncomendaRepository : IEncomendaRepository
    {
        private readonly AppDbContext _context;

        public EncomendaRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Encomenda>> GetAllAsync()
        {
            return await _context.Encomendas
                .Include(e => e.Cliente)
                .Include(e => e.Itens)
                    .ThenInclude(i => i.Produto)
                .OrderByDescending(e => e.DataEncomenda)
                .ToListAsync();
        }

        public async Task<IEnumerable<Encomenda>> GetByClienteIdAsync(string clienteId)
        {
            return await _context.Encomendas
                .Include(e => e.Itens)
                    .ThenInclude(i => i.Produto)
                .Where(e => e.ClienteId == clienteId)
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

        public async Task<Encomenda> CreateAsync(Encomenda encomenda)
        {
            _context.Encomendas.Add(encomenda);
            await _context.SaveChangesAsync();
            return encomenda;
        }

        public async Task UpdateEstadoAsync(int id, EstadoEncomenda novoEstado)
        {
            var encomenda = await _context.Encomendas
                .Include(e => e.Itens)
                    .ThenInclude(i => i.Produto)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (encomenda == null) return;

            var estadoAntigo = encomenda.Estado;
            if (estadoAntigo == novoEstado) return;

            // Validar stock antes de expedir
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

            // Decrementar stock ao expedir
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

            // Repor stock se cancelar após expedir
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
        }

        public async Task<int> GetUnidadesVendidasPorProdutoAsync(int produtoId)
        {
            // Soma as quantidades de todas as encomendas que não foram canceladas
            return await _context.Set<DetalheEncomenda>()
                .Include(d => d.Encomenda)
                .Where(d => d.ProdutoId == produtoId && d.Encomenda.Estado != EstadoEncomenda.Cancelada)
                .SumAsync(d => d.Quantidade);
        }
    }
}