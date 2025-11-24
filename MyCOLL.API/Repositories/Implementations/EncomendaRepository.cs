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

        public async Task<Encomenda> CreateAsync(Encomenda encomenda)
        {
            _context.Encomendas.Add(encomenda);

            await _context.SaveChangesAsync();
            return encomenda;
        }
    }
}