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

        public ProdutosController(IProdutoRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Lista produtos ativos (público)
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Produto>>> GetAll([FromQuery] int? categoriaId)
        {
            IEnumerable<Produto> produtos;

            if (categoriaId.HasValue && categoriaId.Value > 0)
                produtos = await _repository.GetByCategoriaAsync(categoriaId.Value);
            else
                produtos = await _repository.GetAllAsync();

            return Ok(produtos);
        }

        /// <summary>
        /// Obtém produto por ID (público)
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<Produto>> GetById(int id)
        {
            var produto = await _repository.GetByIdAsync(id);
            if (produto == null)
                return NotFound(new { message = "Produto não encontrado" });
            return Ok(produto);
        }

        /// <summary>
        /// Pesquisa produtos por nome/descrição (público)
        /// </summary>
        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Produto>>> Search([FromQuery] string q)
        {
            if (string.IsNullOrWhiteSpace(q))
                return Ok(await _repository.GetAllAsync());

            var produtos = await _repository.SearchAsync(q);
            return Ok(produtos);
        }

        /// <summary>
        /// Produto em destaque aleatório (público)
        /// </summary>
        [HttpGet("destaque")]
        [AllowAnonymous]
        public async Task<ActionResult<Produto>> GetDestaque()
        {
            var produto = await _repository.GetRandomAsync();
            if (produto == null)
                return NotFound(new { message = "Nenhum produto disponível" });
            return Ok(produto);
        }

        /// <summary>
        /// Lista produtos do fornecedor autenticado
        /// </summary>
        [HttpGet("meus")]
        [Authorize(Roles = "Fornecedor")]
        public async Task<ActionResult<IEnumerable<Produto>>> GetMeusProdutos()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Utilizador não autenticado" });

            var produtos = await _repository.GetByFornecedorIdAsync(userId);
            return Ok(produtos);
        }

        /// <summary>
        /// Cria novo produto (Fornecedor/Admin/Gestor)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Fornecedor,Admin,Gestor")]
        public async Task<ActionResult<Produto>> Create([FromBody] ProdutoCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isFornecedor = User.IsInRole("Fornecedor");

            var produto = new Produto
            {
                Nome = dto.Nome,
                Descricao = dto.Descricao,
                PrecoBase = dto.PrecoBase,
                MargemLucro = dto.MargemLucro,
                Preco = dto.PrecoBase + (dto.PrecoBase * (dto.MargemLucro / 100)),
                Stock = dto.Stock,
                CategoriaId = dto.CategoriaId,
                ModoEntregaId = dto.ModoEntregaId,
                ImagemUrl = dto.ImagemUrl,
                FornecedorId = isFornecedor ? userId : null,
                Ativo = !isFornecedor, // Fornecedor fica pendente (RN01)
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

        /// <summary>
        /// Atualiza produto (dono, Admin ou Gestor)
        /// </summary>
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

            // Verificar se é o dono ou admin
            if (!isAdmin && produtoExistente.FornecedorId != userId)
                return Forbid();

            produtoExistente.Nome = dto.Nome;
            produtoExistente.Descricao = dto.Descricao;
            produtoExistente.PrecoBase = dto.PrecoBase;
            produtoExistente.MargemLucro = dto.MargemLucro;
            produtoExistente.Preco = dto.PrecoBase + (dto.PrecoBase * (dto.MargemLucro / 100));
            produtoExistente.Stock = dto.Stock;
            produtoExistente.CategoriaId = dto.CategoriaId;
            produtoExistente.ModoEntregaId = dto.ModoEntregaId;
            produtoExistente.ImagemUrl = dto.ImagemUrl;
            produtoExistente.DataAtualizacao = DateTime.Now;

            await _repository.UpdateAsync(produtoExistente);

            return Ok(new { message = "Produto atualizado com sucesso!" });
        }

        /// <summary>
        /// Elimina produto (dono, Admin ou Gestor)
        /// </summary>
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

        /// <summary>
        /// Ativa/desativa produto (Admin/Gestor apenas)
        /// </summary>
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

            return Ok(new { 
                message = produto.Ativo ? "Produto ativado!" : "Produto desativado!",
                ativo = produto.Ativo
            });
        }
    }
}