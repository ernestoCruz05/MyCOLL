using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyCOLL.API.Entities;
using MyCOLL.API.Repositories.Interfaces;

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
    }
}