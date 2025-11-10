using Cobrio.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cobrio.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Apenas usuários autenticados
public class AdminController : ControllerBase
{
    private readonly CobrioDbContext _context;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        CobrioDbContext context,
        ILogger<AdminController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Sincroniza módulos e permissões - adiciona apenas o que está faltando
    /// </summary>
    /// <remarks>
    /// Este endpoint é idempotente e pode ser executado múltiplas vezes sem problemas.
    /// Ele irá:
    /// 1. Adicionar módulos que ainda não existem
    /// 2. Adicionar ações que ainda não existem
    /// 3. Adicionar permissões faltantes para todas as empresas
    /// </remarks>
    [HttpPost("sync-permissions")]
    public async Task<IActionResult> SyncPermissions()
    {
        try
        {
            _logger.LogInformation("Iniciando sincronização de módulos e permissões...");

            var seeder = new PermissaoSeeder(_context);
            await seeder.SyncModulosEPermissoesAsync();

            _logger.LogInformation("Sincronização concluída com sucesso!");

            return Ok(new
            {
                success = true,
                message = "Módulos e permissões sincronizados com sucesso!"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao sincronizar módulos e permissões");
            return StatusCode(500, new
            {
                success = false,
                message = "Erro ao sincronizar módulos e permissões",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Verifica o status das permissões do sistema
    /// </summary>
    [HttpGet("permissions-status")]
    public async Task<IActionResult> GetPermissionsStatus()
    {
        try
        {
            var totalModulos = await _context.Modulos.CountAsync();
            var totalAcoes = await _context.Acoes.CountAsync();
            var totalEmpresas = await _context.EmpresasCliente.CountAsync();
            var totalPermissoes = await _context.PermissoesPerfil.CountAsync();

            var modulosExistentes = await _context.Modulos
                .Select(m => new { m.Chave, m.Nome })
                .ToListAsync();

            return Ok(new
            {
                totalModulos,
                totalAcoes,
                totalEmpresas,
                totalPermissoes,
                modulosExistentes
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter status de permissões");
            return StatusCode(500, new
            {
                success = false,
                message = "Erro ao obter status de permissões",
                error = ex.Message
            });
        }
    }
}
