using Microsoft.AspNetCore.Mvc;
using MyCOLL.API.Entities;
using MyCOLL.API.Repositories.Interfaces;

namespace MyCOLL.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriasController : ControllerBase
    {
        private readonly ICategoriaRepository _repository;

        public CategoriasController(ICategoriaRepository repository)
        {
            _repository = repository;
        }

        // GET: api/categorias
        // Atualizado para retornar IEnumerable
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Categoria>>> GetAll()
        {
            var categorias = await _repository.GetAllAsync();
            return Ok(categorias);
        }

        // GET: api/categorias/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Categoria>> GetById(int id)
        {
            var categoria = await _repository.GetByIdAsync(id);
            if (categoria == null) return NotFound();
            return Ok(categoria);
        }
    }
}