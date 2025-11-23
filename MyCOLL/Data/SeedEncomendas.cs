using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyCOLL.Entities;

namespace MyCOLL.Data
{
    public static class SeedEncomendas
    {
        public static async Task SeedAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            // 1. Verifica se já existem encomendas. Se sim, sai.
            if (context.Encomendas.Any()) return;

            // 2. Garantir Cliente de Teste
            var clienteEmail = "cliente@teste.com";
            var cliente = await userManager.FindByEmailAsync(clienteEmail);
            if (cliente == null)
            {
                cliente = new ApplicationUser
                {
                    UserName = clienteEmail,
                    Email = clienteEmail,
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(cliente, "Cliente123!");
                await userManager.AddToRoleAsync(cliente, "Cliente");
            }

            // 3. Garantir que existe pelo menos uma Categoria
            var categoria = await context.Categorias.FirstOrDefaultAsync();
            if (categoria == null)
            {
                categoria = new Categoria { Nome = "Numismática", Descricao = "Moedas e notas", Ativa = true };
                context.Categorias.Add(categoria);
                await context.SaveChangesAsync(); // Guardar para gerar ID
            }

            // 4. Garantir que existe pelo menos um Modo de Entrega
            var modoEntrega = await context.ModosEntrega.FirstOrDefaultAsync();
            if (modoEntrega == null)
            {
                modoEntrega = new ModoEntrega
                {
                    Nome = "CTT Expresso",
                    Descricao = "Entrega dia seguinte",
                    CustoBase = 5.00m,
                    PrazoEstimadoDias = 1,
                    Ativo = true
                };
                context.ModosEntrega.Add(modoEntrega);
                await context.SaveChangesAsync(); // Guardar para gerar ID
            }

            // 5. Garantir que existem Produtos
            if (!context.Produtos.Any())
            {
                var p1 = new Produto
                {
                    Nome = "Moeda 2€ Comemorativa",
                    Descricao = "Edição Jogos Olímpicos",
                    Preco = 15.00m,
                    Stock = 10,
                    CategoriaId = categoria.Id,
                    ModoEntregaId = modoEntrega.Id,
                    Ativo = true
                };

                var p2 = new Produto
                {
                    Nome = "Selo D. Afonso Henriques",
                    Descricao = "Selo raro de 1950",
                    Preco = 120.00m,
                    Stock = 2,
                    CategoriaId = categoria.Id,
                    ModoEntregaId = modoEntrega.Id,
                    Ativo = true
                };

                context.Produtos.AddRange(p1, p2);
                await context.SaveChangesAsync();
            }

            // Recarregar produtos da DB para ter os IDs corretos
            var produtos = await context.Produtos.ToListAsync();

            // 6. Criar Encomenda Pendente
            var encomenda1 = new Encomenda
            {
                ClienteId = cliente.Id,
                DataEncomenda = DateTime.Now.AddDays(-1),
                Estado = EstadoEncomenda.Pendente,
                MoradaEnvio = "Rua de Teste, 123, Lisboa",
                MetodoEntregaNome = modoEntrega.Nome,
                CustoEntrega = modoEntrega.CustoBase,
                Total = 0,
                Itens = new List<DetalheEncomenda>()
            };

            decimal total1 = 0;
            foreach (var prod in produtos.Take(2))
            {
                encomenda1.Itens.Add(new DetalheEncomenda
                {
                    ProdutoId = prod.Id,
                    Quantidade = 1,
                    PrecoUnitario = prod.Preco
                });
                total1 += prod.Preco;
            }
            encomenda1.Total = total1 + encomenda1.CustoEntrega;
            context.Encomendas.Add(encomenda1);

            // 7. Criar Encomenda Paga
            var encomenda2 = new Encomenda
            {
                ClienteId = cliente.Id,
                DataEncomenda = DateTime.Now.AddHours(-5),
                Estado = EstadoEncomenda.Paga,
                MoradaEnvio = "Avenida dos Aliados, Porto",
                MetodoEntregaNome = modoEntrega.Nome,
                CustoEntrega = modoEntrega.CustoBase,
                Total = 0,
                Itens = new List<DetalheEncomenda>()
            };

            var prodUnico = produtos.LastOrDefault();
            if (prodUnico != null)
            {
                encomenda2.Itens.Add(new DetalheEncomenda
                {
                    ProdutoId = prodUnico.Id,
                    Quantidade = 1,
                    PrecoUnitario = prodUnico.Preco
                });
                encomenda2.Total = prodUnico.Preco + encomenda2.CustoEntrega;
                context.Encomendas.Add(encomenda2);
            }

            await context.SaveChangesAsync();
        }
    }
}