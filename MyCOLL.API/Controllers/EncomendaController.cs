using Microsoft.AspNetCore.Mvc;
using MyCOLL.API.DTOs;
using MyCOLL.API.Entities;
using MyCOLL.API.Repositories.Interfaces;

namespace MyCOLL.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EncomendasController : ControllerBase
    {
        private readonly IEncomendaRepository _encomendaRepo;
        private readonly IProdutoRepository _produtoRepo;
        private readonly IModoEntregaRepository _modoEntregaRepo;

        public EncomendasController(
            IEncomendaRepository encomendaRepo,
            IProdutoRepository produtoRepo,
            IModoEntregaRepository modoEntregaRepo)
        {
            _encomendaRepo = encomendaRepo;
            _produtoRepo = produtoRepo;
            _modoEntregaRepo = modoEntregaRepo;
        }

        [HttpPost]
        public async Task<ActionResult<Encomenda>> Create([FromBody] EncomendaCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // 1. Validar Modo de Entrega e obter custo
            var modoEntrega = await _modoEntregaRepo.GetByIdAsync(dto.ModoEntregaId);
            if (modoEntrega == null) return BadRequest("Modo de entrega inválido.");

            // 2. Construir a Entidade Encomenda
            var novaEncomenda = new Encomenda
            {
                ClienteId = dto.ClienteId,
                DataEncomenda = DateTime.Now,
                Estado = EstadoEncomenda.Pendente, // Começa sempre pendente
                MoradaEnvio = dto.MoradaEnvio,
                MetodoEntregaNome = modoEntrega.Nome,
                CustoEntrega = modoEntrega.CustoBase,
                Itens = new List<DetalheEncomenda>()
            };

            decimal totalProdutos = 0;

            // 3. Processar Itens (Validar stock e obter preço atual)
            foreach (var itemDto in dto.Itens)
            {
                var produto = await _produtoRepo.GetByIdAsync(itemDto.ProdutoId);

                if (produto == null)
                    return BadRequest($"Produto {itemDto.ProdutoId} não encontrado.");

                if (!produto.Ativo)
                    return BadRequest($"Produto '{produto.Nome}' não está disponível.");

                // Opcional: Validar Stock aqui
                // if (produto.Stock < itemDto.Quantidade) return BadRequest(...)

                var detalhe = new DetalheEncomenda
                {
                    ProdutoId = produto.Id,
                    Quantidade = itemDto.Quantidade,
                    PrecoUnitario = produto.Preco // Usa o preço DA BASE DE DADOS, nunca do JSON
                };

                novaEncomenda.Itens.Add(detalhe);
                totalProdutos += (detalhe.PrecoUnitario * detalhe.Quantidade);
            }

            // 4. Calcular Total Final
            novaEncomenda.Total = totalProdutos + novaEncomenda.CustoEntrega;

            // 5. Guardar
            await _encomendaRepo.CreateAsync(novaEncomenda);

            // Retornar 201 Created com o ID da nova encomenda
            return CreatedAtAction(nameof(Create), new { id = novaEncomenda.Id }, novaEncomenda);
        }
    }
}