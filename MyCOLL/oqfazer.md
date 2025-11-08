Estrutura do Projeto
-------------------------------------------

Data/           -> Base de dados (DbContext e ApplicationUser)
Entities/       -> Classes que representam tabelas da BD (ex: Categoria, Produto)
Services/       -> Classes que ligam o Blazor à Base de Dados (usam o DbContext)
Pages/          -> Páginas Blazor (.razor) — parte visual do projeto (UI)
Components/     -> Layouts, menus e componentes partilhados
appsettings.json -> Configurações da base de dados (connection string)
Program.cs      -> Configuração de serviços e inicialização do projeto


Fluxo da Aplicação
-------------------------------------------

1. O utilizador interage com uma página .razor
2. A página usa um Service (ex: CategoriaService) para obter dados
3. O Service usa o ApplicationDbContext para aceder à Base de Dados
4. O Service devolve os dados para a página .razor

Resumo:
UI (.razor) -> Service -> DbContext -> Base de Dados


Como adicionar algo novo (exemplo: nova entidade)
-------------------------------------------

1️ - Criar a entidade:
-------------------------------------------
Cria um ficheiro em Entities/, ex: Fornecedor.cs

public class Fornecedor {
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
}

2 - Atualizar o ApplicationDbContext:
-------------------------------------------
Adiciona:
public DbSet<Fornecedor> Fornecedores { get; set; }

3️ - Criar o Service:
-------------------------------------------
Cria FornecedorService.cs em Services/
(segue o mesmo padrão dos outros serviços — CRUD completo)

4️ - Registar o Service em Program.cs:
-------------------------------------------
builder.Services.AddScoped<FornecedorService>();

5️ - Criar as Páginas Blazor:
-------------------------------------------
Cria a pasta Pages/Fornecedores/ com:
- Index.razor (listar e eliminar)
- Create.razor (criar novo)
- Edit.razor (editar existente)

6️ - Adicionar ao menu lateral (NavMenu.razor):
-------------------------------------------
<div class="nav-item px-3">
  <NavLink class="nav-link" href="/fornecedores">
    <span class="bi bi-people"></span> Fornecedores
  </NavLink>
</div>
*/

Outras Dicas
-------------------------------------------

- Se a página não reconhecer uma classe (ex: Categoria), adiciona:
  @using MyCOLL.Entities

- Se quiseres usar um novo Service:
  @inject MyCOLL.Services.ProdutoService ProdutoService

- Sempre que mudares entidades ou o contexto:
  Add-Migration NomeDaMigration
  Update-Database

