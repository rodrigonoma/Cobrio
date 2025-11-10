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
            new Modulo("Regras de Cobrança", "regras-cobranca", "Configurar regras de cobrança", "pi-bell", "/regras-cobranca", 4),
            new Modulo("Usuários", "usuarios", "Gerenciar usuários do sistema", "pi-user-edit", "/usuarios", 5),
            new Modulo("Templates", "templates", "Gerenciar templates de email", "pi-file", "/templates", 6),
            new Modulo("Relatórios", "relatorios", "Visualizar relatórios e dashboards", "pi-chart-bar", "/relatorios", 7),
            new Modulo("Relatórios Operacionais", "relatorios-operacionais", "Relatórios operacionais e execução de cobranças", "pi-chart-line", "/relatorios", 8),
            new Modulo("Relatórios Gerenciais", "relatorios-gerenciais", "Relatórios gerenciais e análises estratégicas", "pi-chart-pie", "/relatorios", 9),
            new Modulo("Configurações", "configuracoes", "Configurar sistema (email, integrações, etc)", "pi-cog", "/configuracoes/email", 10),
            new Modulo("Permissões", "permissoes", "Configurar permissões de perfis (Proprietário)", "pi-shield", "/permissoes", 11)
        };

        await _context.Modulos.AddRangeAsync(modulos);

        // Criar Ações
        var acoes = new List<Acao>
        {
            // Ações de Menu
            new Acao("Visualizar Menu", "menu.view", "Visualizar item no menu lateral", TipoAcao.Menu),

            // Ações CRUD
            new Acao("Listar", "read", "Visualizar listagem", TipoAcao.CRUD),
            new Acao("Visualizar", "visualizar", "Visualizar conteúdo", TipoAcao.CRUD),
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
            ["regras-cobranca"] = new[] { "menu.view", "read", "read.details", "create", "update", "delete", "export", "import" },
            ["usuarios"] = new[] { "menu.view", "read", "read.details", "create", "update", "delete", "reset-password" },
            ["templates"] = new[] { "menu.view", "read", "read.details", "create", "update", "delete" },
            ["relatorios"] = new[] { "menu.view", "read", "export" },
            ["relatorios-operacionais"] = new[] { "read", "export" },
            ["relatorios-gerenciais"] = new[] { "read", "export" },
            ["configuracoes"] = new[] { "menu.view", "read", "update" },
            ["permissoes"] = new[] { "menu.view", "read", "config-permissions" }
        };

        var permissoes = new List<PermissaoPerfil>();

        // ADMIN: Acesso completo a todos os módulos (exceto Permissões e Configurações - apenas Proprietário)
        foreach (var modulo in modulos.Where(m => m.Chave != "permissoes" && m.Chave != "configuracoes"))
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

    /// <summary>
    /// Sincroniza módulos e permissões - adiciona apenas o que está faltando (idempotente)
    /// </summary>
    public async Task SyncModulosEPermissoesAsync()
    {
        // 1. Sincronizar Módulos
        var modulosDefinidos = new List<(string Nome, string Chave, string Descricao, string Icone, string Rota, int Ordem)>
        {
            ("Dashboard", "dashboard", "Painel principal com métricas", "pi-home", "/dashboard", 1),
            ("Assinaturas", "assinaturas", "Gerenciar assinaturas de clientes", "pi-users", "/assinaturas", 2),
            ("Planos", "planos", "Gerenciar planos de oferta", "pi-tag", "/planos", 3),
            ("Regras de Cobrança", "regras-cobranca", "Configurar regras de cobrança", "pi-bell", "/regras-cobranca", 4),
            ("Usuários", "usuarios", "Gerenciar usuários do sistema", "pi-user-edit", "/usuarios", 5),
            ("Templates", "templates", "Gerenciar templates de email", "pi-file", "/templates", 6),
            ("Relatórios", "relatorios", "Visualizar relatórios e dashboards", "pi-chart-bar", "/relatorios", 7),
            ("Relatórios Operacionais", "relatorios-operacionais", "Relatórios operacionais e execução de cobranças", "pi-chart-line", "/relatorios", 8),
            ("Relatórios Gerenciais", "relatorios-gerenciais", "Relatórios gerenciais e análises estratégicas", "pi-chart-pie", "/relatorios", 9),
            ("Configurações", "configuracoes", "Configurar sistema (email, integrações, etc)", "pi-cog", "/configuracoes/email", 10),
            ("Permissões", "permissoes", "Configurar permissões de perfis (Proprietário)", "pi-shield", "/permissoes", 11)
        };

        var modulosExistentes = await _context.Modulos.Select(m => m.Chave).ToListAsync();
        var modulosFaltando = modulosDefinidos.Where(m => !modulosExistentes.Contains(m.Chave)).ToList();

        if (modulosFaltando.Any())
        {
            var novosModulos = modulosFaltando.Select(m => new Modulo(m.Nome, m.Chave, m.Descricao, m.Icone, m.Rota, m.Ordem)).ToList();
            await _context.Modulos.AddRangeAsync(novosModulos);
            await _context.SaveChangesAsync();
        }

        // 2. Sincronizar Ações (se necessário)
        var acoesDefinidas = new List<(string Nome, string Chave, string Descricao, TipoAcao Tipo)>
        {
            ("Visualizar Menu", "menu.view", "Visualizar item no menu lateral", TipoAcao.Menu),
            ("Listar", "read", "Visualizar listagem", TipoAcao.CRUD),
            ("Visualizar", "visualizar", "Visualizar conteúdo", TipoAcao.CRUD),
            ("Visualizar Detalhes", "read.details", "Visualizar detalhes de um item", TipoAcao.CRUD),
            ("Criar", "create", "Criar novo registro", TipoAcao.CRUD),
            ("Editar", "update", "Editar registro existente", TipoAcao.CRUD),
            ("Deletar", "delete", "Deletar registro", TipoAcao.CRUD),
            ("Ativar/Desativar", "toggle", "Ativar ou desativar registro", TipoAcao.CRUD),
            ("Exportar", "export", "Exportar dados", TipoAcao.Especial),
            ("Importar", "import", "Importar dados", TipoAcao.Especial),
            ("Resetar Senha", "reset-password", "Resetar senha de usuário", TipoAcao.Especial),
            ("Configurar Permissões", "config-permissions", "Configurar permissões de perfis", TipoAcao.Especial)
        };

        var acoesExistentes = await _context.Acoes.Select(a => a.Chave).ToListAsync();
        var acoesFaltando = acoesDefinidas.Where(a => !acoesExistentes.Contains(a.Chave)).ToList();

        if (acoesFaltando.Any())
        {
            var novasAcoes = acoesFaltando.Select(a => new Acao(a.Nome, a.Chave, a.Descricao, a.Tipo)).ToList();
            await _context.Acoes.AddRangeAsync(novasAcoes);
            await _context.SaveChangesAsync();
        }

        // 3. Sincronizar Permissões para TODAS as empresas
        var moduloAcoesMap = new Dictionary<string, string[]>
        {
            ["dashboard"] = new[] { "menu.view", "read" },
            ["assinaturas"] = new[] { "menu.view", "read", "read.details", "create", "update", "delete", "export" },
            ["planos"] = new[] { "menu.view", "read", "read.details", "create", "update", "delete", "toggle", "export" },
            ["regras-cobranca"] = new[] { "menu.view", "read", "read.details", "create", "update", "delete", "export", "import" },
            ["usuarios"] = new[] { "menu.view", "read", "read.details", "create", "update", "delete", "reset-password" },
            ["templates"] = new[] { "menu.view", "read", "read.details", "create", "update", "delete" },
            ["relatorios"] = new[] { "menu.view", "read", "export" },
            ["relatorios-operacionais"] = new[] { "read", "export" },
            ["relatorios-gerenciais"] = new[] { "read", "export" },
            ["configuracoes"] = new[] { "menu.view", "read", "update" },
            ["permissoes"] = new[] { "menu.view", "read", "config-permissions" }
        };

        var modulos = await _context.Modulos.ToListAsync();
        var acoes = await _context.Acoes.ToListAsync();

        // Para cada empresa
        var empresas = await _context.EmpresasCliente
            .Include(e => e.Usuarios.Where(u => u.EhProprietario))
            .ToListAsync();

        foreach (var empresa in empresas)
        {
            var proprietario = empresa.Usuarios.FirstOrDefault(u => u.EhProprietario);
            if (proprietario == null) continue;

            // Obter permissões existentes desta empresa
            var permissoesExistentes = await _context.PermissoesPerfil
                .Where(p => p.EmpresaClienteId == empresa.Id)
                .Select(p => new { p.PerfilUsuario, p.ModuloId, p.AcaoId })
                .ToListAsync();

            var novasPermissoes = new List<PermissaoPerfil>();

            // ADMIN: Acesso a todos os módulos (exceto Permissões e Configurações)
            foreach (var modulo in modulos.Where(m => m.Chave != "permissoes" && m.Chave != "configuracoes"))
            {
                if (!moduloAcoesMap.TryGetValue(modulo.Chave, out var acoesValidas))
                    continue;

                foreach (var acaoChave in acoesValidas)
                {
                    var acao = acoes.FirstOrDefault(a => a.Chave == acaoChave);
                    if (acao == null) continue;

                    // Verificar se já existe
                    if (permissoesExistentes.Any(p =>
                        p.PerfilUsuario == PerfilUsuario.Admin &&
                        p.ModuloId == modulo.Id &&
                        p.AcaoId == acao.Id))
                        continue;

                    novasPermissoes.Add(new PermissaoPerfil(
                        empresa.Id,
                        PerfilUsuario.Admin,
                        modulo.Id,
                        acao.Id,
                        true,
                        proprietario.Id
                    ));
                }
            }

            // OPERADOR: Apenas templates e regras de cobrança (visualização)
            var modulosOperador = new[] { "templates", "regras-cobranca" };
            var acoesOperador = new[] { "menu.view", "read", "read.details" };

            foreach (var chaveModulo in modulosOperador)
            {
                var modulo = modulos.FirstOrDefault(m => m.Chave == chaveModulo);
                if (modulo == null) continue;

                foreach (var chaveAcao in acoesOperador)
                {
                    var acao = acoes.FirstOrDefault(a => a.Chave == chaveAcao);
                    if (acao == null) continue;

                    // Verificar se já existe
                    if (permissoesExistentes.Any(p =>
                        p.PerfilUsuario == PerfilUsuario.Operador &&
                        p.ModuloId == modulo.Id &&
                        p.AcaoId == acao.Id))
                        continue;

                    novasPermissoes.Add(new PermissaoPerfil(
                        empresa.Id,
                        PerfilUsuario.Operador,
                        modulo.Id,
                        acao.Id,
                        true,
                        proprietario.Id
                    ));
                }
            }

            // Adicionar permissões negadas para Operador em templates (create, update, delete)
            var templatesModulo = modulos.FirstOrDefault(m => m.Chave == "templates");
            if (templatesModulo != null)
            {
                var acoesNegadas = new[] { "create", "update", "delete" };
                foreach (var chaveAcao in acoesNegadas)
                {
                    var acao = acoes.FirstOrDefault(a => a.Chave == chaveAcao);
                    if (acao == null) continue;

                    if (permissoesExistentes.Any(p =>
                        p.PerfilUsuario == PerfilUsuario.Operador &&
                        p.ModuloId == templatesModulo.Id &&
                        p.AcaoId == acao.Id))
                        continue;

                    novasPermissoes.Add(new PermissaoPerfil(
                        empresa.Id,
                        PerfilUsuario.Operador,
                        templatesModulo.Id,
                        acao.Id,
                        false, // Não permitido
                        proprietario.Id
                    ));
                }
            }

            if (novasPermissoes.Any())
            {
                await _context.PermissoesPerfil.AddRangeAsync(novasPermissoes);
            }
        }

        await _context.SaveChangesAsync();
    }
}
