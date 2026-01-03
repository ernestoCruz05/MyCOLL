using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyCOLL.API.DTOs;
using MyCOLL.API.Entities;
using MyCOLL.API.Repositories.Interfaces;
using System.Security.Claims;

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

        [HttpGet]
        [Authorize(Roles = "Admin,Gestor")]
        public async Task<ActionResult<IEnumerable<Encomenda>>> GetAll()
        {
            var encomendas = await _encomendaRepo.GetAllAsync();
            return Ok(encomendas);
        }

        [HttpGet("minhas")]
        [Authorize(Roles = "Cliente,Fornecedor")]
        public async Task<ActionResult<IEnumerable<Encomenda>>> GetMinhasEncomendas()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Utilizador não autenticado" });

            var encomendas = await _encomendaRepo.GetByClienteIdAsync(userId);
            return Ok(encomendas);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<Encomenda>> GetById(int id)
        {
            var encomenda = await _encomendaRepo.GetByIdAsync(id);
            if (encomenda == null)
                return NotFound(new { message = "Encomenda não encontrada" });

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin") || User.IsInRole("Gestor");

            if (!isAdmin && encomenda.ClienteId != userId)
                return Forbid();

            return Ok(encomenda);
        }

        [HttpPost]
        [Authorize(Roles = "Cliente,Fornecedor")]
        public async Task<ActionResult<Encomenda>> Create([FromBody] EncomendaCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // SEGURANÇA: Obter ID do utilizador do TOKEN!
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Utilizador não autenticado" });

            var modoEntrega = await _modoEntregaRepo.GetByIdAsync(dto.ModoEntregaId);
            if (modoEntrega == null || !modoEntrega.Ativo)
                return BadRequest(new { message = "Modo de entrega inválido" });

            if (dto.Itens == null || !dto.Itens.Any())
                return BadRequest(new { message = "A encomenda deve ter pelo menos 1 item" });

            var novaEncomenda = new Encomenda
            {
                ClienteId = userId,
                DataEncomenda = DateTime.Now,
                Estado = EstadoEncomenda.Pendente,
                MoradaEnvio = dto.MoradaEnvio,
                MetodoEntregaNome = modoEntrega.Nome,
                CustoEntrega = modoEntrega.CustoBase,
                Itens = new List<DetalheEncomenda>()
            };

            decimal totalProdutos = 0;

            foreach (var itemDto in dto.Itens)
            {
                if (itemDto.Quantidade <= 0)
                    return BadRequest(new { message = "Quantidade deve ser maior que zero" });

                var produto = await _produtoRepo.GetByIdAsync(itemDto.ProdutoId);

                if (produto == null)
                    return BadRequest(new { message = $"Produto {itemDto.ProdutoId} não encontrado" });

                if (!produto.Ativo)
                    return BadRequest(new { message = $"Produto '{produto.Nome}' não está disponível" });

                if (produto.Stock < itemDto.Quantidade)
                    return BadRequest(new { message = $"Stock insuficiente para '{produto.Nome}'. Disponível: {produto.Stock}" });

                var detalhe = new DetalheEncomenda
                {
                    ProdutoId = produto.Id,
                    Quantidade = itemDto.Quantidade,
                    PrecoUnitario = produto.Preco
                };

                novaEncomenda.Itens.Add(detalhe);
                totalProdutos += (detalhe.PrecoUnitario * detalhe.Quantidade);
            }

            novaEncomenda.Total = totalProdutos + novaEncomenda.CustoEntrega;

            await _encomendaRepo.CreateAsync(novaEncomenda);

            return CreatedAtAction(nameof(GetById), new { id = novaEncomenda.Id }, new
            {
                id = novaEncomenda.Id,
                total = novaEncomenda.Total,
                estado = novaEncomenda.Estado.ToString(),
                message = "Encomenda criada com sucesso!"
            });
        }

        [HttpPatch("{id}/estado")]
        [Authorize(Roles = "Admin,Gestor")]
        public async Task<IActionResult> UpdateEstado(int id, [FromBody] UpdateEstadoDto dto)
        {
            var encomenda = await _encomendaRepo.GetByIdAsync(id);
            if (encomenda == null)
                return NotFound(new { message = "Encomenda não encontrada" });

            if (!IsValidTransition(encomenda.Estado, dto.NovoEstado))
                return BadRequest(new { message = $"Transição de '{encomenda.Estado}' para '{dto.NovoEstado}' não é permitida" });

            try
            {
                await _encomendaRepo.UpdateEstadoAsync(id, dto.NovoEstado);
                return Ok(new { message = $"Estado atualizado para '{dto.NovoEstado}'" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        private static bool IsValidTransition(EstadoEncomenda atual, EstadoEncomenda novo)
        {
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

    public class UpdateEstadoDto
    {
        public EstadoEncomenda NovoEstado { get; set; }
    }
}