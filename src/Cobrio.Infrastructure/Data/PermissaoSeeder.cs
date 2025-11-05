using Cobrio.Domain.Entities;
using Cobrio.Domain.Enums;
using Microsoft.EntityFrameworkCore;

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
        // Verificar se já existem permissões para esta empresa
        if (_context.PermissoesPerfil.Any(p => p.EmpresaClienteId == empresaClienteId))
        {
            return;
        }

        var modulos = await _context.Modulos.ToListAsync();
        var acoes = await _context.Acoes.ToListAsync();

        // Mapear quais ações são válidas para cada módulo
        var moduloAcoesMap = new Dictionary<string, string[]>
        {
            ["dashboard"] = new[] { "menu.view", "read" },
            ["assinaturas"] = new[] { "menu.view", "read", "read.details", "create", "update", "delete", "export" },
            ["planos"] = new[] { "menu.view", "read", "read.details", "create", "update", "delete", "toggle", "export" },
            ["financeiro"] = new[] { "menu.view", "read", "read.details", "export" },
            ["regras-cobranca"] = new[] { "menu.view", "read", "read.details", "create", "update", "delete", "export", "import" },
            ["usuarios"] = new[] { "menu.view", "read", "read.details", "create", "update", "delete", "reset-password" },
            ["relatorios"] = new[] { "menu.view", "read", "export" },
            ["permissoes"] = new[] { "menu.view", "read", "config-permissions" }
        };

        var permissoes = new List<PermissaoPerfil>();

        // ADMIN: Acesso completo a todos os módulos (exceto Permissões)
        foreach (var modulo in modulos.Where(m => m.Chave != "permissoes"))
        {
            // Obter ações válidas para este módulo
            if (!moduloAcoesMap.TryGetValue(modulo.Chave, out var acoesValidas))
                continue;

            foreach (var acaoChave in acoesValidas)
            {
                var acao = acoes.FirstOrDefault(a => a.Chave == acaoChave);
                if (acao == null) continue;

                permissoes.Add(new PermissaoPerfil(
                    empresaClienteId,
                    PerfilUsuario.Admin,
                    modulo.Id,
                    acao.Id,
                    true, // Admin tem acesso total
                    proprietarioId
                ));
            }
        }

        // OPERADOR: Apenas visualizar Regras de Cobrança
        var regrasModulo = modulos.First(m => m.Chave == "regras-cobranca");
        var menuViewAction = acoes.First(a => a.Chave == "menu.view");
        var readAction = acoes.First(a => a.Chave == "read");

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
