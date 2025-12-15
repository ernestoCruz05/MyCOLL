using Microsoft.EntityFrameworkCore;
using MyCOLL.API.Entities;

namespace MyCOLL.API.Data
{
    public static class SeedCategorias
    {
        public static async Task SeedAsync(AppDbContext context, string baseUrl)
        {
            var todasCategorias = new List<(string Nome, string Descricao)>
            {
                ("Numismática", "Moedas, notas e medalhas"),
                ("Selos", "Filatelia e coleção de selos"),
                ("Antiguidades", "Objetos antigos e vintage"),
                ("Arte", "Pinturas, esculturas e gravuras"),
                ("Jogos de Cartas Colecionáveis", "TCG, Magic, Pokémon, Yu-Gi-Oh"),
                ("Modelismo", "Modelos à escala e maquetes"),
                ("Veículos", "Carros, motas e miniaturas"),
                ("BD", "Banda desenhada e comics"),
                ("Livros", "Livros raros e edições especiais"),
                ("Roupa", "Roupa vintage e de coleção")
            };

            var categoriasExistentes = await context.Categorias.ToListAsync();

            foreach (var (nome, descricao) in todasCategorias)
            {
                var categoriaExistente = categoriasExistentes
                    .FirstOrDefault(c => c.Nome.Equals(nome, StringComparison.OrdinalIgnoreCase));

                if (categoriaExistente == null)
                {
                    context.Categorias.Add(new Categoria
                    {
                        Nome = nome,
                        Descricao = descricao,
                        ImagemUrl = null, // Images will be uploaded via the UI
                        Ativa = true,
                        DataCriacao = DateTime.UtcNow
                    });
                }
            }

            await context.SaveChangesAsync();
        }
    }
}