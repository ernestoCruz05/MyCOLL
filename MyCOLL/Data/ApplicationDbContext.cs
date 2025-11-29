using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyCOLL.Entities;

namespace MyCOLL.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Produto> Produtos { get; set; }
        public DbSet<ModoEntrega> ModosEntrega { get; set; }
        public DbSet<LogEntry> Logs { get; set; }
        public DbSet<Encomenda> Encomendas { get; set; }
        public DbSet<DetalheEncomenda> DetalhesEncomenda { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Aqui estamos a enforçar regras na base de dados
            // Nos ficheiros .cs usamos tambem as vezes cenas tipo [Column(TypeName = "decimal(18,2)")] mas isso é para a aplicação e não para a base de dados diretamente
            // 1 - Impedir apagar uma Categoria se ela tiver Produtos
            builder.Entity<Produto>()
                .HasOne(p => p.Categoria)
                .WithMany(c => c.Produtos)
                .HasForeignKey(p => p.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);

            // 2 - Impedir apagar um Produto se ja tiver sido comprado (Em encomendas)
            builder.Entity<DetalheEncomenda>()
                .HasOne(d => d.Produto)
                .WithMany()
                .HasForeignKey(d => d.ProdutoId)
                .OnDelete(DeleteBehavior.Restrict);

            // 3 - Definir a precisão decimal
            builder.Entity<Produto>()
                .Property(p => p.PrecoBase).HasColumnType("decimal(18,2)");
            builder.Entity<Produto>()
                .Property(p => p.MargemLucro).HasColumnType("decimal(18,2)");
            builder.Entity<Produto>()
                .Property(p => p.Preco).HasColumnType("decimal(18,2)");
            builder.Entity<ModoEntrega>()
                .Property(m => m.CustoBase).HasColumnType("decimal(18,2)");
            builder.Entity<Encomenda>()
                .Property(e => e.Total).HasColumnType("decimal(18,2)");
            builder.Entity<DetalheEncomenda>()
                .Property(d => d.PrecoUnitario).HasColumnType("decimal(18,2)");
        }
    }
}
