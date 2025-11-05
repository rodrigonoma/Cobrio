using Cobrio.Application.Interfaces;
using Cobrio.Domain.Entities;
using Cobrio.Domain.Enums;
using Cobrio.Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Cobrio.Application.Services;

public class PermissaoService : IPermissaoService
{
    private readonly IPermissaoRepository _permissaoRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMemoryCache _cache;
    private readonly ILogger<PermissaoService> _logger;

    private const int CACHE_DURATION_MINUTES = 30;

    public PermissaoService(
        IPermissaoRepository permissaoRepository,
        IUnitOfWork unitOfWork,
        IMemoryCache cache,
        ILogger<PermissaoService> logger)
    {
        _permissaoRepository = permissaoRepository;
        _unitOfWork = unitOfWork;
        _cache = cache;
        _logger = logger;
    }

    public async Task<IEnumerable<Modulo>> GetModulosAtivosAsync(CancellationToken cancellationToken = default)
    {
        const string cacheKey = "modulos_ativos";

        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CACHE_DURATION_MINUTES);
            _logger.LogInformation("Carregando módulos ativos do banco de dados");
            return await _permissaoRepository.GetModulosAtivosAsync(cancellationToken);
        }) ?? Enumerable.Empty<Modulo>();
    }

    public async Task<IEnumerable<Acao>> GetAcoesAtivasAsync(CancellationToken cancellationToken = default)
    {
        const string cacheKey = "acoes_ativas";

        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CACHE_DURATION_MINUTES);
            _logger.LogInformation("Carregando ações ativas do banco de dados");
            return await _permissaoRepository.GetAcoesAtivasAsync(cancellationToken);
        }) ?? Enumerable.Empty<Acao>();
    }

    public async Task<IEnumerable<PermissaoPerfil>> GetPermissoesByPerfilAsync(
        Guid empresaClienteId,
        PerfilUsuario perfil,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = GetPermissoesCacheKey(empresaClienteId, perfil);

        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CACHE_DURATION_MINUTES);
            _logger.LogInformation(
                "Carregando permissões do perfil {Perfil} da empresa {EmpresaId}",
                perfil,
                empresaClienteId);
            return await _permissaoRepository.GetPermissoesByPerfilAsync(
                empresaClienteId,
                perfil,
                cancellationToken);
        }) ?? Enumerable.Empty<PermissaoPerfil>();
    }

    public async Task<bool> TemPermissaoAsync(
        Guid empresaClienteId,
        PerfilUsuario perfil,
        string moduloChave,
        string acaoChave,
        CancellationToken cancellationToken = default)
    {
        // Buscar da lista completa de permissões (que tem cache próprio e é limpo ao salvar)
        // Isso evita problema de cache desatualizado nas permissões individuais
        var permissoes = await GetPermissoesByPerfilAsync(empresaClienteId, perfil, cancellationToken);

        var permissao = permissoes.FirstOrDefault(p =>
            p.Modulo?.Chave == moduloChave &&
            p.Acao?.Chave == acaoChave);

        return permissao?.Permitido ?? false;
    }

    public async Task ConfigurarPermissoesAsync(
        Guid empresaClienteId,
        PerfilUsuario perfil,
        Dictionary<Guid, Dictionary<Guid, bool>> permissoes,
        Guid usuarioId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Configurando permissões para perfil {Perfil} da empresa {EmpresaId}",
            perfil,
            empresaClienteId);

        var permissoesParaSalvar = new List<PermissaoPerfil>();

        foreach (var (moduloId, acoesPermissoes) in permissoes)
        {
            foreach (var (acaoId, permitido) in acoesPermissoes)
            {
                var permissao = new PermissaoPerfil(
                    empresaClienteId,
                    perfil,
                    moduloId,
                    acaoId,
                    permitido,
                    usuarioId);

                permissoesParaSalvar.Add(permissao);
            }
        }

        await _permissaoRepository.UpsertPermissoesEmLoteAsync(permissoesParaSalvar, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        // Limpar cache após salvar
        await LimparCachePermissoesAsync(empresaClienteId, perfil);

        _logger.LogInformation(
            "Permissões configuradas com sucesso para perfil {Perfil}. Total: {Total}",
            perfil,
            permissoesParaSalvar.Count);
    }

    public Task LimparCachePermissoesAsync(Guid empresaClienteId, PerfilUsuario perfil)
    {
        var cacheKeyPattern = GetPermissoesCacheKey(empresaClienteId, perfil);
        _cache.Remove(cacheKeyPattern);

        _logger.LogInformation(
            "Cache de permissões limpo para perfil {Perfil} da empresa {EmpresaId}",
            perfil,
            empresaClienteId);

        return Task.CompletedTask;
    }

    private static string GetPermissoesCacheKey(Guid empresaClienteId, PerfilUsuario perfil)
    {
        return $"permissoes_{empresaClienteId}_{perfil}";
    }
}
