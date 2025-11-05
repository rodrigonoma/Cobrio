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
