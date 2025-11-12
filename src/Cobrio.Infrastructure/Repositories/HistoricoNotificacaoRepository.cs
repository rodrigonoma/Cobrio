using Cobrio.Domain.Entities;
using Cobrio.Domain.Enums;
using Cobrio.Domain.Interfaces;
using Cobrio.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Cobrio.Infrastructure.Repositories;

public class HistoricoNotificacaoRepository : Repository<HistoricoNotificacao>, IHistoricoNotificacaoRepository
{
    private readonly ICurrentUserService _currentUserService;

    public HistoricoNotificacaoRepository(CobrioDbContext context, ICurrentUserService currentUserService) : base(context)
    {
        _currentUserService = currentUserService;
    }

    public async Task<IEnumerable<HistoricoNotificacao>> GetByCobrancaIdAsync(Guid cobrancaId, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(h => h.CobrancaId == cobrancaId);

        // Aplicar filtros de segurança baseado no perfil do usuário
        query = ApplySecurityFilters(query);

        return await query
            .OrderByDescending(h => h.DataEnvio)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<HistoricoNotificacao>> GetByRegraIdAsync(Guid regraId, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(h => h.RegraCobrancaId == regraId);

        // Aplicar filtros de segurança baseado no perfil do usuário
        query = ApplySecurityFilters(query);

        return await query
            .OrderByDescending(h => h.DataEnvio)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<HistoricoNotificacao>> GetByStatusAsync(StatusNotificacao status, DateTime dataInicio, DateTime dataFim, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(h => h.Status == status && h.DataEnvio >= dataInicio && h.DataEnvio <= dataFim);

        // Aplicar filtros de segurança baseado no perfil do usuário
        query = ApplySecurityFilters(query);

        return await query
            .OrderByDescending(h => h.DataEnvio)
            .ToListAsync(cancellationToken);
    }

    public async Task<HistoricoNotificacao?> GetByMessageIdProvedor(string messageId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(messageId))
            return null;

        // IMPORTANTE: IgnoreQueryFilters() para permitir busca cross-tenant
        // Isso é necessário porque o webhook do Brevo não tem contexto de tenant
        return await _dbSet
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(h => h.MessageIdProvedor == messageId, cancellationToken);
    }

    public async Task<HistoricoNotificacao?> GetByEmailEDataAsync(string email, DateTime dataReferencia, int toleranciaMinutos = 30, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        var dataInicio = dataReferencia.AddMinutes(-toleranciaMinutos);
        var dataFim = dataReferencia.AddMinutes(toleranciaMinutos);

        // IMPORTANTE: IgnoreQueryFilters() para permitir busca cross-tenant
        // Busca o histórico mais recente de email enviado nesse período
        // Busca tanto no PayloadUtilizado quanto no PayloadJson da Cobranca
        return await _dbSet
            .IgnoreQueryFilters()
            .Include(h => h.Cobranca)
            .Where(h =>
                h.CanalUtilizado == CanalNotificacao.Email &&
                h.DataEnvio >= dataInicio &&
                h.DataEnvio <= dataFim &&
                (h.PayloadUtilizado.Contains(email) || h.Cobranca.PayloadJson.Contains(email)))
            .OrderByDescending(h => h.DataEnvio)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<HistoricoNotificacao>> GetByFiltrosAsync(
        Guid empresaClienteId,
        DateTime? dataInicio,
        DateTime? dataFim,
        StatusNotificacao? status,
        string? emailDestinatario,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Include(h => h.Cobranca)
            .Include(h => h.RegraCobranca)
            .Where(h => h.EmpresaClienteId == empresaClienteId);

        // Aplicar filtros de segurança baseado no perfil do usuário
        query = ApplySecurityFilters(query);

        if (dataInicio.HasValue)
            query = query.Where(h => h.DataEnvio >= dataInicio.Value);

        if (dataFim.HasValue)
            query = query.Where(h => h.DataEnvio <= dataFim.Value);

        if (status.HasValue)
            query = query.Where(h => h.Status == status.Value);

        if (!string.IsNullOrWhiteSpace(emailDestinatario))
            query = query.Where(h => h.Cobranca.PayloadJson.Contains(emailDestinatario));

        return await query
            .OrderByDescending(h => h.DataEnvio)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Aplica filtros de segurança baseado no perfil do usuário:
    /// - Operador: vê apenas seus registros
    /// - Admin: vê seus registros + de operadores (não vê de outros admins)
    /// - Proprietário: vê tudo
    /// </summary>
    private IQueryable<HistoricoNotificacao> ApplySecurityFilters(IQueryable<HistoricoNotificacao> query)
    {
        // Proprietário vê tudo
        if (_currentUserService.EhProprietario)
            return query;

        // Se não está autenticado, retorna vazio
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
            return query.Where(h => false);

        var userId = _currentUserService.UserId.Value;
        var perfil = _currentUserService.Perfil;

        // Operador vê apenas o que ele criou
        if (perfil == PerfilUsuario.Operador)
        {
            return query.Where(h => h.UsuarioCriacaoId == userId);
        }

        // Admin vê:
        // 1. Seus próprios registros
        // 2. Registros sem dono (dados antigos antes da auditoria)
        // 3. Registros criados por operadores (precisa fazer JOIN com UsuarioEmpresa para verificar perfil)
        if (perfil == PerfilUsuario.Admin)
        {
            return query.Where(h =>
                h.UsuarioCriacaoId == userId ||  // Seus próprios
                !h.UsuarioCriacaoId.HasValue     // Dados antigos (sem dono)
                // TODO: Adicionar filtro para ver registros de operadores
                // Isso requer JOIN com UsuarioEmpresa, o que pode impactar performance
                // Por ora, admin vê apenas seus próprios + dados antigos
            );
        }

        // Fallback: retorna apenas dados do próprio usuário
        return query.Where(h => h.UsuarioCriacaoId == userId || !h.UsuarioCriacaoId.HasValue);
    }
}
