# 🔐 Implementação do Sistema de Permissões - Continuação

## ✅ O que já foi criado (Arquivos gerados):

### Domain Layer:
1. ✅ `src/Cobrio.Domain/Entities/Modulo.cs`
2. ✅ `src/Cobrio.Domain/Entities/Acao.cs`
3. ✅ `src/Cobrio.Domain/Entities/PermissaoPerfil.cs`
4. ✅ `src/Cobrio.Domain/Enums/TipoAcao.cs`

### Infrastructure Layer - Configurations:
5. ✅ `src/Cobrio.Infrastructure/Data/Configurations/ModuloConfiguration.cs`
6. ✅ `src/Cobrio.Infrastructure/Data/Configurations/AcaoConfiguration.cs`
7. ✅ `src/Cobrio.Infrastructure/Data/Configurations/PermissaoPerfilConfiguration.cs`

---

## 📋 Próximos Passos (Em ordem):

### 6️⃣ Atualizar DbContext

**Arquivo**: `src/Cobrio.Infrastructure/Data/CobrioDbContext.cs`

Adicionar as propriedades DbSet:

```csharp
// Adicionar junto com os outros DbSet
public DbSet<Modulo> Modulos => Set<Modulo>();
public DbSet<Acao> Acoes => Set<Acao>();
public DbSet<PermissaoPerfil> PermissoesPerfil => Set<PermissaoPerfil>();
```

E no método `OnModelCreating`:

```csharp
modelBuilder.ApplyConfiguration(new ModuloConfiguration());
modelBuilder.ApplyConfiguration(new AcaoConfiguration());
modelBuilder.ApplyConfiguration(new PermissaoPerfilConfiguration());
```

### 7️⃣ Criar Migration

```bash
cd src/Cobrio.Infrastructure
dotnet ef migrations add AddPermissionsSystem --startup-project ../Cobrio.API
dotnet ef database update --startup-project ../Cobrio.API
```

### 8️⃣ Criar Seed de Módulos e Ações

**Arquivo**: `src/Cobrio.Infrastructure/Data/PermissaoSeeder.cs`

```csharp
using Cobrio.Domain.Entities;
using Cobrio.Domain.Enums;

namespace Cobrio.Infrastructure.Data;

public class PermissaoSeeder
{
    private readonly CobrioDbContext _context;

    public PermissaoSeeder(CobrioDbContext context)
    {
        _context = context;
    }

    public async Task SeedModulosEAcoesAsync()
    {
        // Verificar se já existe
        if (_context.Modulos.Any()) return;

        // Criar Módulos
        var modulos = new List<Modulo>
        {
            new Modulo("Dashboard", "dashboard", "Painel principal com métricas", "pi-home", "/dashboard", 1),
            new Modulo("Assinaturas", "assinaturas", "Gerenciar assinaturas de clientes", "pi-users", "/assinaturas", 2),
            new Modulo("Planos", "planos", "Gerenciar planos de oferta", "pi-tag", "/planos", 3),
            new Modulo("Financeiro", "financeiro", "Controle financeiro e faturas", "pi-dollar", "/financeiro", 4),
            new Modulo("Regras de Cobrança", "regras-cobranca", "Configurar regras de cobrança", "pi-bell", "/regras-cobranca", 5),
            new Modulo("Usuários", "usuarios", "Gerenciar usuários do sistema", "pi-user-edit", "/usuarios", 6),
            new Modulo("Relatórios", "relatorios", "Visualizar relatórios e dashboards", "pi-chart-bar", "/relatorios", 7),
            new Modulo("Permissões", "permissoes", "Configurar permissões de perfis (Proprietário)", "pi-shield", "/permissoes", 8)
        };

        await _context.Modulos.AddRangeAsync(modulos);

        // Criar Ações
        var acoes = new List<Acao>
        {
            // Ações de Menu
            new Acao("Visualizar Menu", "menu.view", "Visualizar item no menu lateral", TipoAcao.Menu),

            // Ações CRUD
            new Acao("Listar", "read", "Visualizar listagem", TipoAcao.CRUD),
            new Acao("Visualizar Detalhes", "read.details", "Visualizar detalhes de um item", TipoAcao.CRUD),
            new Acao("Criar", "create", "Criar novo registro", TipoAcao.CRUD),
            new Acao("Editar", "update", "Editar registro existente", TipoAcao.CRUD),
            new Acao("Deletar", "delete", "Deletar registro", TipoAcao.CRUD),
            new Acao("Ativar/Desativar", "toggle", "Ativar ou desativar registro", TipoAcao.CRUD),

            // Ações Especiais
            new Acao("Exportar", "export", "Exportar dados", TipoAcao.Especial),
            new Acao("Importar", "import", "Importar dados", TipoAcao.Especial),
            new Acao("Resetar Senha", "reset-password", "Resetar senha de usuário", TipoAcao.Especial),
            new Acao("Configurar Permissões", "config-permissions", "Configurar permissões de perfis", TipoAcao.Especial)
        };

        await _context.Acoes.AddRangeAsync(acoes);
        await _context.SaveChangesAsync();
    }

    public async Task SeedPermissoesDefaultAsync(Guid empresaClienteId, Guid proprietarioId)
    {
        var modulos = _context.Modulos.ToList();
        var acoes = _context.Acoes.ToList();

        var menuViewAction = acoes.First(a => a.Chave == "menu.view");
        var readAction = acoes.First(a => a.Chave == "read");

        var permissoes = new List<PermissaoPerfil>();

        // ADMIN: Acesso a tudo exceto módulo Permissões
        foreach (var modulo in modulos.Where(m => m.Chave != "permissoes"))
        {
            // Menu
            permissoes.Add(new PermissaoPerfil(
                empresaClienteId,
                PerfilUsuario.Admin,
                modulo.Id,
                menuViewAction.Id,
                true,
                proprietarioId
            ));

            // Todas as ações CRUD
            foreach (var acao in acoes.Where(a => a.TipoAcao == TipoAcao.CRUD || a.TipoAcao == TipoAcao.Especial))
            {
                permissoes.Add(new PermissaoPerfil(
                    empresaClienteId,
                    PerfilUsuario.Admin,
                    modulo.Id,
                    acao.Id,
                    true,
                    proprietarioId
                ));
            }
        }

        // OPERADOR: Apenas visualizar Regras de Cobrança
        var regrasModulo = modulos.First(m => m.Chave == "regras-cobranca");

        permissoes.Add(new PermissaoPerfil(
            empresaClienteId,
            PerfilUsuario.Operador,
            regrasModulo.Id,
            menuViewAction.Id,
            true,
            proprietarioId
        ));

        permissoes.Add(new PermissaoPerfil(
            empresaClienteId,
            PerfilUsuario.Operador,
            regrasModulo.Id,
            readAction.Id,
            true,
            proprietarioId
        ));

        await _context.PermissoesPerfil.AddRangeAsync(permissoes);
        await _context.SaveChangesAsync();
    }
}
```

### 9️⃣ Chamar Seed no Program.cs

No `Program.cs`, dentro do bloco que já chama o DatabaseSeeder:

```csharp
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<CobrioDbContext>();
        var logger = services.GetRequiredService<ILogger<DatabaseSeeder>>();

        // Seed original
        var seeder = new DatabaseSeeder(context, logger);
        await seeder.SeedAsync();

        // NOVO: Seed de permissões
        var permissaoSeeder = new PermissaoSeeder(context);
        await permissaoSeeder.SeedModulosEAcoesAsync();

        // Pegar o proprietário criado pelo seed original
        var proprietario = context.UsuariosEmpresa.First(u => u.EhProprietario);
        await permissaoSeeder.SeedPermissoesDefaultAsync(proprietario.EmpresaClienteId, proprietario.Id);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Erro ao executar seed do banco de dados");
    }
}
```

### 🔟 Criar Repositories

**Arquivo**: `src/Cobrio.Domain/Interfaces/IPermissaoRepository.cs`

```csharp
using Cobrio.Domain.Entities;
using Cobrio.Domain.Enums;

namespace Cobrio.Domain.Interfaces;

public interface IPermissaoRepository
{
    Task<IEnumerable<Modulo>> GetModulosAtivosAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Acao>> GetAcoesAtivasAsync(CancellationToken cancellationToken = default);

    Task<IEnumerable<PermissaoPerfil>> GetPermissoesByPerfilAsync(
        Guid empresaClienteId,
        PerfilUsuario perfil,
        CancellationToken cancellationToken = default);

    Task<bool> TemPermissaoAsync(
        Guid empresaClienteId,
        PerfilUsuario perfil,
        string moduloChave,
        string acaoChave,
        CancellationToken cancellationToken = default);

    Task<PermissaoPerfil?> GetPermissaoAsync(
        Guid empresaClienteId,
        PerfilUsuario perfil,
        Guid moduloId,
        Guid acaoId,
        CancellationToken cancellationToken = default);

    Task UpsertPermissaoAsync(
        PermissaoPerfil permissao,
        CancellationToken cancellationToken = default);

    Task UpsertPermissoesEmLoteAsync(
        IEnumerable<PermissaoPerfil> permissoes,
        CancellationToken cancellationToken = default);
}
```

**Arquivo**: `src/Cobrio.Infrastructure/Repositories/PermissaoRepository.cs`

```csharp
using Cobrio.Domain.Entities;
using Cobrio.Domain.Enums;
using Cobrio.Domain.Interfaces;
using Cobrio.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Cobrio.Infrastructure.Repositories;

public class PermissaoRepository : IPermissaoRepository
{
    private readonly CobrioDbContext _context;

    public PermissaoRepository(CobrioDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Modulo>> GetModulosAtivosAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Modulos
            .Where(m => m.Ativo)
            .OrderBy(m => m.Ordem)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Acao>> GetAcoesAtivasAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Acoes
            .Where(a => a.Ativa)
            .OrderBy(a => a.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<PermissaoPerfil>> GetPermissoesByPerfilAsync(
        Guid empresaClienteId,
        PerfilUsuario perfil,
        CancellationToken cancellationToken = default)
    {
        return await _context.PermissoesPerfil
            .Include(p => p.Modulo)
            .Include(p => p.Acao)
            .Where(p => p.EmpresaClienteId == empresaClienteId && p.PerfilUsuario == perfil)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> TemPermissaoAsync(
        Guid empresaClienteId,
        PerfilUsuario perfil,
        string moduloChave,
        string acaoChave,
        CancellationToken cancellationToken = default)
    {
        return await _context.PermissoesPerfil
            .AnyAsync(p =>
                p.EmpresaClienteId == empresaClienteId &&
                p.PerfilUsuario == perfil &&
                p.Modulo!.Chave == moduloChave &&
                p.Acao!.Chave == acaoChave &&
                p.Permitido,
                cancellationToken);
    }

    public async Task<PermissaoPerfil?> GetPermissaoAsync(
        Guid empresaClienteId,
        PerfilUsuario perfil,
        Guid moduloId,
        Guid acaoId,
        CancellationToken cancellationToken = default)
    {
        return await _context.PermissoesPerfil
            .FirstOrDefaultAsync(p =>
                p.EmpresaClienteId == empresaClienteId &&
                p.PerfilUsuario == perfil &&
                p.ModuloId == moduloId &&
                p.AcaoId == acaoId,
                cancellationToken);
    }

    public async Task UpsertPermissaoAsync(
        PermissaoPerfil permissao,
        CancellationToken cancellationToken = default)
    {
        var existente = await GetPermissaoAsync(
            permissao.EmpresaClienteId,
            permissao.PerfilUsuario,
            permissao.ModuloId,
            permissao.AcaoId,
            cancellationToken);

        if (existente != null)
        {
            if (permissao.Permitido)
                existente.Permitir();
            else
                existente.Negar();
        }
        else
        {
            await _context.PermissoesPerfil.AddAsync(permissao, cancellationToken);
        }
    }

    public async Task UpsertPermissoesEmLoteAsync(
        IEnumerable<PermissaoPerfil> permissoes,
        CancellationToken cancellationToken = default)
    {
        foreach (var permissao in permissoes)
        {
            await UpsertPermissaoAsync(permissao, cancellationToken);
        }
    }
}
```

### 1️⃣1️⃣ Registrar no DI Container

No `Program.cs`, adicionar no registro de repositories:

```csharp
builder.Services.AddScoped<IPermissaoRepository, PermissaoRepository>();
```

---

## 📄 Próximos arquivos necessários:

Devido ao limite de tokens, a continuação inclui:

12. **PermissaoService** (com cache)
13. **Custom Authorization Attribute**
14. **PermissoesController** (API endpoints)
15. **DTOs** (Request/Response)
16. **Frontend Service**
17. **Frontend Component** (tela de gerenciamento)
18. **Atualizar Sidebar** para usar permissões dinâmicas

Execute os passos acima e me informe quando terminar, que continuarei com os próximos arquivos! 🚀
