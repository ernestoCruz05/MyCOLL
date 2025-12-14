using Microsoft.EntityFrameworkCore;
using MyCOLL.Data;
using MyCOLL.Entities;

namespace MyCOLL.Services
{
    public class ProdutoService
    {
        private readonly ApplicationDbContext _context;
        private readonly LogService _log;

        public ProdutoService(ApplicationDbContext context, LogService log)
        {
            _context = context;
            _log = log;
        }

        public async Task<List<Produto>> GetAllAsync() =>
            await _context.Produtos
                .Include(p => p.Categoria)
                .Include(p => p.ModoEntrega)
                .Include(p => p.Fornecedor)
                .OrderByDescending(p => p.Id)
                .ToListAsync();

        public async Task<Produto?> GetByIdAsync(int id) =>
            await _context.Produtos
                .Include(p => p.Categoria)
                .Include(p => p.ModoEntrega)
                .Include(p => p.Fornecedor)
                .FirstOrDefaultAsync(p => p.Id == id);

        public async Task AddAsync(Produto produto)
        {
            // Calcular preço final
            produto.Preco = produto.PrecoBase + (produto.PrecoBase * (produto.MargemLucro / 100));
            produto.DataCriacao = DateTime.Now;
            produto.DataAtualizacao = null;

            // RN01: Produtos de fornecedores ficam inativos até aprovação
            if (!string.IsNullOrEmpty(produto.FornecedorId))
            {
                produto.Ativo = false;
            }

            _context.Produtos.Add(produto);
            await _context.SaveChangesAsync();
            await _log.AddAsync("Produto", "Criado", produto.Nome);
        }

        public async Task UpdateAsync(Produto produto)
        {
            // Recalcular preço final
            produto.Preco = produto.PrecoBase + (produto.PrecoBase * (produto.MargemLucro / 100));
            produto.DataAtualizacao = DateTime.Now;

            _context.Produtos.Update(produto);
            await _context.SaveChangesAsync();
            await _log.AddAsync("Produto", "Atualizado", produto.Nome);
        }

        public async Task DeleteAsync(int id)
        {
            var prod = await _context.Produtos.FindAsync(id);
            if (prod == null) return;

            // RN02: Verificar se existe em alguma encomenda
            var temEncomendas = await _context.DetalhesEncomenda
                .AnyAsync(d => d.ProdutoId == id);

            if (temEncomendas)
            {
                throw new InvalidOperationException(
                    $"Não é possível eliminar '{prod.Nome}' porque já existem encomendas com este produto. " +
                    "Considere desativar o produto em vez de eliminar.");
            }

            _context.Produtos.Remove(prod);
            await _context.SaveChangesAsync();
            await _log.AddAsync("Produto", "Eliminado", prod.Nome);
        }

        /// <summary>
        /// Ativa ou desativa um produto (soft delete / aprovação)
        /// </summary>
        public async Task ToggleAtivoAsync(int id)
        {
            var produto = await _context.Produtos.FindAsync(id);
            if (produto == null)
                throw new InvalidOperationException("Produto não encontrado.");

            produto.Ativo = !produto.Ativo;
            produto.DataAtualizacao = DateTime.Now;

            await _context.SaveChangesAsync();
            await _log.AddAsync("Produto", produto.Ativo ? "Ativado" : "Desativado", produto.Nome);
        }
    }
}