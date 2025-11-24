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


        [HttpGet]
        public async Task<ActionResult<IEnumerable<Produto>>> GetAll([FromQuery] int? categoriaId)
        {
            IEnumerable<Produto> produtos;

            if (categoriaId.HasValue && categoriaId.Value > 0)
            {
                produtos = await _repository.GetByCategoriaAsync(categoriaId.Value);
            }
            else
            {
                produtos = await _repository.GetAllAsync();
            }

            return Ok(produtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Produto>> GetById(int id)
        {
            var produto = await _repository.GetByIdAsync(id);
            if (produto == null) return NotFound();
            return Ok(produto);
        }
    }
}