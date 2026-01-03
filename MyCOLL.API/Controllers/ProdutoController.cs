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
    public class ProdutosController : ControllerBase
    {
        private readonly IProdutoRepository _repository;
        private readonly IEncomendaRepository _encomendaRepository;

        public ProdutosController(IProdutoRepository repository, IEncomendaRepository encomendaRepository)
        {
            _repository = repository;
            _encomendaRepository = encomendaRepository;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Produto>>> GetAll([FromQuery] int? categoriaId, [FromQuery] bool includeInactive = true)
        {
            IEnumerable<Produto> produtos;

            if (categoriaId.HasValue && categoriaId.Value > 0)
            {
                produtos = includeInactive 
                    ? await _repository.GetByCategoriaIncludingInactiveAsync(categoriaId.Value)
                    : await _repository.GetByCategoriaAsync(categoriaId.Value);
            }
            else
            {
                produtos = includeInactive 
                    ? await _repository.GetAllIncludingInactiveAsync()
                    : await _repository.GetAllAsync();
            }

            return Ok(produtos);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<Produto>> GetById(int id, [FromQuery] bool includeInactive = true)
        {
            var produto = includeInactive 
                ? await _repository.GetByIdIncludingInactiveAsync(id)
                : await _repository.GetByIdAsync(id);
                
            if (produto == null)
                return NotFound(new { message = "Produto não encontrado" });
            return Ok(produto);
        }

        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Produto>>> Search([FromQuery] string q, [FromQuery] bool includeInactive = true)
        {
            if (string.IsNullOrWhiteSpace(q))
                return Ok(includeInactive 
                    ? await _repository.GetAllIncludingInactiveAsync()
                    : await _repository.GetAllAsync());

            var produtos = includeInactive 
                ? await _repository.SearchIncludingInactiveAsync(q)
                : await _repository.SearchAsync(q);
            return Ok(produtos);
        }

        [HttpGet("destaque")]
        [AllowAnonymous]
        public async Task<ActionResult<Produto>> GetDestaque()
        {
            var produto = await _repository.GetRandomAsync();
            if (produto == null)
                return NotFound(new { message = "Nenhum produto disponível" });
            return Ok(produto);
        }

        [HttpGet("meus")]
        [Authorize(Roles = "Fornecedor")]
        public async Task<ActionResult<IEnumerable<object>>> GetMeusProdutos()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Utilizador não autenticado" });

            var produtos = await _repository.GetByFornecedorIdAsync(userId);

            var produtosComVendas = new List<object>();
            foreach (var produto in produtos)
            {
                var unidadesVendidas = await _encomendaRepository.GetUnidadesVendidasPorProdutoAsync(produto.Id);
                produtosComVendas.Add(new
                {
                    produto.Id,
                    produto.Nome,
                    produto.Descricao,
                    produto.PrecoBase,
                    produto.MargemLucro,
                    produto.Preco,
                    produto.Stock,
                    produto.CategoriaId,
                    produto.Categoria,
                    produto.ModoEntregaId,
                    produto.ModoEntrega,
                    produto.FornecedorId,
                    produto.ImagemUrl,
                    produto.Ativo,
                    produto.DataCriacao,
                    produto.DataAtualizacao,
                    UnidadesVendidas = unidadesVendidas
                });
            }

            return Ok(produtosComVendas);
        }

        [HttpPost]
        [Authorize(Roles = "Fornecedor,Admin,Gestor")]
        public async Task<ActionResult<Produto>> Create([FromBody] ProdutoCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isFornecedor = User.IsInRole("Fornecedor");

            decimal margem = isFornecedor ? 0 : dto.MargemLucro;

            var produto = new Produto
            {
                Nome = dto.Nome,
                Descricao = dto.Descricao,
                PrecoBase = dto.PrecoBase,
                MargemLucro = margem,
                Preco = dto.PrecoBase + (dto.PrecoBase * (margem / 100)),
                Stock = dto.Stock,
                CategoriaId = dto.CategoriaId,
                ModoEntregaId = dto.ModoEntregaId,
                ImagemUrl = dto.ImagemUrl,
                FornecedorId = isFornecedor ? userId : null,
                Ativo = !isFornecedor,
                DataCriacao = DateTime.Now
            };

            await _repository.CreateAsync(produto);

            return CreatedAtAction(nameof(GetById), new { id = produto.Id }, new
            {
                id = produto.Id,
                message = isFornecedor
                    ? "Produto submetido! Aguarde aprovação do administrador."
                    : "Produto criado com sucesso!"
            });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Fornecedor,Admin,Gestor")]
        public async Task<IActionResult> Update(int id, [FromBody] ProdutoUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var produtoExistente = await _repository.GetByIdAsync(id);
            if (produtoExistente == null)
                return NotFound(new { message = "Produto não encontrado" });

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin") || User.IsInRole("Gestor");
            var isFornecedor = User.IsInRole("Fornecedor");

            if (!isAdmin && produtoExistente.FornecedorId != userId)
                return Forbid();

            produtoExistente.Nome = dto.Nome;
            produtoExistente.Descricao = dto.Descricao;
            produtoExistente.PrecoBase = dto.PrecoBase;
            produtoExistente.Stock = dto.Stock;
            produtoExistente.CategoriaId = dto.CategoriaId;
            produtoExistente.ModoEntregaId = dto.ModoEntregaId;
            produtoExistente.ImagemUrl = dto.ImagemUrl;
            produtoExistente.DataAtualizacao = DateTime.Now;

            if (isFornecedor)
            {
                produtoExistente.Ativo = false;
                produtoExistente.Preco = produtoExistente.PrecoBase + (produtoExistente.PrecoBase * (produtoExistente.MargemLucro / 100));
            }
            else
            {
                produtoExistente.MargemLucro = dto.MargemLucro;
                produtoExistente.Preco = produtoExistente.PrecoBase + (produtoExistente.PrecoBase * (dto.MargemLucro / 100));
            }

            await _repository.UpdateAsync(produtoExistente);

            return Ok(new
            {
                message = isFornecedor
                    ? "Produto atualizado e submetido para aprovação."
                    : "Produto atualizado com sucesso!"
            });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Fornecedor,Admin,Gestor")]
        public async Task<IActionResult> Delete(int id)
        {
            var produto = await _repository.GetByIdAsync(id);
            if (produto == null)
                return NotFound(new { message = "Produto não encontrado" });

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin") || User.IsInRole("Gestor");

            if (!isAdmin && produto.FornecedorId != userId)
                return Forbid();

            await _repository.DeleteAsync(id);

            return Ok(new { message = "Produto eliminado com sucesso!" });
        }

        [HttpPatch("{id}/toggle-ativo")]
        [Authorize(Roles = "Admin,Gestor")]
        public async Task<IActionResult> ToggleAtivo(int id)
        {
            var produto = await _repository.GetByIdAsync(id);
            if (produto == null)
                return NotFound(new { message = "Produto não encontrado" });

            produto.Ativo = !produto.Ativo;
            produto.DataAtualizacao = DateTime.Now;

            await _repository.UpdateAsync(produto);

            return Ok(new
            {
                message = produto.Ativo ? "Produto ativado!" : "Produto desativado!",
                ativo = produto.Ativo
            });
        }
    }
}