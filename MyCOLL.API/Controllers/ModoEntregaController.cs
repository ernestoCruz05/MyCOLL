using Microsoft.AspNetCore.Mvc;
using MyCOLL.API.Entities;
using MyCOLL.API.Repositories.Interfaces;

namespace MyCOLL.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ModosEntregaController : ControllerBase
    {
        private readonly IModoEntregaRepository _repository;

        public ModosEntregaController(IModoEntregaRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ModoEntrega>>> GetAll()
        {
            var modos = await _repository.GetAllAsync();
            return Ok(modos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ModoEntrega>> GetById(int id)
        {
            var modo = await _repository.GetByIdAsync(id);
            if (modo == null) return NotFound();
            return Ok(modo);
        }
    }
}