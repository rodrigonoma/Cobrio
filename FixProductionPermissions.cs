using Microsoft.EntityFrameworkCore;
using Cobrio.Infrastructure.Data;
using Cobrio.Domain.Entities;
using Cobrio.Domain.Enums;

// Connection string de produção
var connectionString = "Server=127.0.0.1;Database=Cobrio;User=root;Password=root;CharSet=utf8mb4;Allow User Variables=true;";

var optionsBuilder = new DbContextOptionsBuilder<CobrioDbContext>();
var serverVersion = new MySqlServerVersion(new Version(8, 0, 0));
optionsBuilder.UseMySql(connectionString, serverVersion);

using var context = new CobrioDbContext(optionsBuilder.Options);

Console.WriteLine("===== DIAGNÓSTICO DO BANCO DE PRODUÇÃO =====\n");

// 1. Verificar conexão
try
{
    await context.Database.CanConnectAsync();
    Console.WriteLine("✓ Conexão com o banco de dados estabelecida com sucesso!");
}
catch (Exception ex)
{
    Console.WriteLine($"✗ Erro ao conectar no banco: {ex.Message}");
    return;
}

// 2. Verificar tabelas
Console.WriteLine("\n----- VERIFICANDO TABELAS -----");
var modulos = await context.Modulos.ToListAsync();
var acoes = await context.Acoes.ToListAsync();
var permissoes = await context.PermissoesPerfil.ToListAsync();

Console.WriteLine($"Módulos encontrados: {modulos.Count}");
Console.WriteLine($"Ações encontradas: {acoes.Count}");
Console.WriteLine($"Permissões encontradas: {permissoes.Count}");

// 3. Listar módulos existentes
Console.WriteLine("\n----- MÓDULOS CADASTRADOS -----");
foreach (var modulo in modulos.OrderBy(m => m.Ordem))
{
    Console.WriteLine($"  - {modulo.Nome} (Chave: {modulo.Chave}, Ordem: {modulo.Ordem})");
}

// 4. Listar ações existentes
Console.WriteLine("\n----- AÇÕES CADASTRADAS -----");
foreach (var acao in acoes.OrderBy(a => a.Tipo).ThenBy(a => a.Nome))
{
    Console.WriteLine($"  - {acao.Nome} (Chave: {acao.Chave}, Tipo: {acao.Tipo})");
}

// 5. Verificar módulos faltando
Console.WriteLine("\n----- VERIFICANDO MÓDULOS FALTANDO -----");
var modulosEsperados = new Dictionary<string, string>
{
    ["dashboard"] = "Dashboard",
    ["assinaturas"] = "Assinaturas",
    ["planos"] = "Planos",
    ["regras-cobranca"] = "Regras de Cobrança",
    ["usuarios"] = "Usuários",
    ["templates"] = "Templates",
    ["relatorios"] = "Relatórios",
    ["relatorios-operacionais"] = "Relatórios Operacionais",
    ["relatorios-gerenciais"] = "Relatórios Gerenciais",
    ["configuracoes"] = "Configurações",
    ["permissoes"] = "Permissões"
};

var modulosFaltando = modulosEsperados.Where(me => !modulos.Any(m => m.Chave == me.Key)).ToList();
if (modulosFaltando.Any())
{
    Console.WriteLine("MÓDULOS FALTANDO:");
    foreach (var item in modulosFaltando)
    {
        Console.WriteLine($"  ✗ {item.Value} ({item.Key})");
    }
}
else
{
    Console.WriteLine("✓ Todos os módulos esperados estão cadastrados!");
}

// 6. Verificar ações faltando
Console.WriteLine("\n----- VERIFICANDO AÇÕES FALTANDO -----");
var acoesEsperadas = new[] {
    "menu.view", "read", "visualizar", "read.details", "create", "update",
    "delete", "toggle", "export", "import", "reset-password", "config-permissions"
};

var acoesFaltando = acoesEsperadas.Where(ae => !acoes.Any(a => a.Chave == ae)).ToList();
if (acoesFaltando.Any())
{
    Console.WriteLine("AÇÕES FALTANDO:");
    foreach (var item in acoesFaltando)
    {
        Console.WriteLine($"  ✗ {item}");
    }
}
else
{
    Console.WriteLine("✓ Todas as ações esperadas estão cadastradas!");
}

// 7. Verificar permissões por perfil
Console.WriteLine("\n----- PERMISSÕES POR PERFIL -----");
var permissoesPorPerfil = permissoes
    .GroupBy(p => p.Perfil)
    .Select(g => new {
        Perfil = g.Key,
        Total = g.Count(),
        Permitidas = g.Count(p => p.Permitido),
        Negadas = g.Count(p => !p.Permitido)
    })
    .ToList();

foreach (var item in permissoesPorPerfil)
{
    Console.WriteLine($"  {item.Perfil}: {item.Total} permissões ({item.Permitidas} permitidas, {item.Negadas} negadas)");
}

if (!permissoesPorPerfil.Any())
{
    Console.WriteLine("  ✗ NENHUMA PERMISSÃO CADASTRADA!");
}

// 8. Verificar usuário proprietário
Console.WriteLine("\n----- VERIFICANDO USUÁRIO PROPRIETÁRIO -----");
var proprietarios = await context.UsuariosEmpresa
    .Where(u => u.EhProprietario)
    .ToListAsync();

if (proprietarios.Any())
{
    foreach (var prop in proprietarios)
    {
        Console.WriteLine($"  ✓ {prop.Nome} ({prop.Email}) - EmpresaId: {prop.EmpresaClienteId}");
    }
}
else
{
    Console.WriteLine("  ✗ NENHUM PROPRIETÁRIO ENCONTRADO!");
}

// 9. Perguntar se deve corrigir
Console.WriteLine("\n\n===== AÇÕES CORRETIVAS =====");
Console.WriteLine("Deseja executar o seed para corrigir os dados? (S/N)");
var resposta = Console.ReadLine()?.ToUpper();

if (resposta == "S" || resposta == "SIM")
{
    Console.WriteLine("\nExecutando seed de permissões...");

    try
    {
        // Executar seed de módulos e ações
        var seeder = new PermissaoSeeder(context);
        await seeder.SeedModulosEAcoesAsync();
        Console.WriteLine("✓ Módulos e ações criados/atualizados com sucesso!");

        // Se houver proprietário, executar seed de permissões
        if (proprietarios.Any())
        {
            var proprietario = proprietarios.First();
            await seeder.SeedPermissoesDefaultAsync(proprietario.EmpresaClienteId, proprietario.Id);
            Console.WriteLine($"✓ Permissões padrão criadas para a empresa {proprietario.EmpresaClienteId}!");
        }
        else
        {
            Console.WriteLine("⚠ Não foi possível criar permissões pois não há proprietário cadastrado.");
        }

        Console.WriteLine("\n✓ CORREÇÃO CONCLUÍDA COM SUCESSO!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\n✗ ERRO ao executar seed: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
    }
}
else
{
    Console.WriteLine("Operação cancelada.");
}

Console.WriteLine("\nPressione qualquer tecla para sair...");
Console.ReadKey();
