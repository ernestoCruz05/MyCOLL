using MyCOLL.Data;
using MyCOLL.Entities;
using Microsoft.EntityFrameworkCore;

namespace MyCOLL.Services
{
    public class LogService
    {
        private readonly ApplicationDbContext _context;

        public LogService(ApplicationDbContext context)
        {
            _context = context;
        }

        // novo: inclui "nome"
        public async Task AddAsync(string tipo, string acao, string nome, string? descricao = null)
        {
            var log = new LogEntry
            {
                Tipo = tipo,
                Acao = acao,
                Nome = nome,
                Descricao = descricao ?? string.Empty,
                Data = DateTime.Now
            };

            _context.Logs.Add(log);
            await _context.SaveChangesAsync();
        }

        public Task AddAsync(string tipo, string acao, string descricao)
            => AddAsync(tipo, acao, nome: string.Empty, descricao: descricao);

        public async Task<List<LogEntry>> GetRecent(int limit = 10)
        {
            return await _context.Logs
                .OrderByDescending(l => l.Data)
                .Take(limit)
                .ToListAsync();
        }
    }
}
