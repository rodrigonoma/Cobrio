using Cobrio.Application.DTOs.RegraCobranca;
using Cobrio.Application.Interfaces;
using Cobrio.Domain.Entities;
using Cobrio.Domain.Interfaces;

namespace Cobrio.Application.Services;

public class RegraCobrancaService : IRegraCobrancaService
{
    private readonly IRegraCobrancaRepository _regraRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RegraCobrancaService(IRegraCobrancaRepository regraRepository, IUnitOfWork unitOfWork)
    {
        _regraRepository = regraRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<RegraCobrancaResponse> CreateAsync(Guid empresaClienteId, CreateRegraCobrancaRequest request, CancellationToken cancellationToken = default)
    {
        var regra = new RegraCobranca(
            empresaClienteId,
            request.Nome,
            request.TipoMomento,
            request.ValorTempo,
            request.UnidadeTempo,
            request.CanalNotificacao,
            request.TemplateNotificacao,
            request.VariaveisObrigatoriasSistema,
            request.Descricao
        );

        await _regraRepository.AddAsync(regra, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return MapToResponse(regra);
    }

    public async Task<RegraCobrancaResponse> UpdateAsync(Guid empresaClienteId, Guid id, UpdateRegraCobrancaRequest request, CancellationToken cancellationToken = default)
    {
        var regra = await _regraRepository.SingleOrDefaultAsync(r => r.Id == id && r.EmpresaClienteId == empresaClienteId, cancellationToken);

        if (regra == null)
            throw new KeyNotFoundException($"Regra de cobrança com ID {id} não encontrada");

        regra.Atualizar(request.Nome, request.Descricao, request.TipoMomento, request.ValorTempo, request.UnidadeTempo, request.CanalNotificacao, request.TemplateNotificacao, request.VariaveisObrigatoriasSistema);

        if (request.Ativa.HasValue)
        {
            if (request.Ativa.Value) regra.Ativar();
            else regra.Desativar();
        }

        _regraRepository.Update(regra);
        await _unitOfWork.CommitAsync(cancellationToken);

        return MapToResponse(regra);
    }

    public async Task<RegraCobrancaResponse> GetByIdAsync(Guid empresaClienteId, Guid id, CancellationToken cancellationToken = default)
    {
        var regra = await _regraRepository.SingleOrDefaultAsync(r => r.Id == id && r.EmpresaClienteId == empresaClienteId, cancellationToken);

        if (regra == null)
            throw new KeyNotFoundException($"Regra de cobrança com ID {id} não encontrada");

        return MapToResponse(regra);
    }

    public async Task<IEnumerable<RegraCobrancaResponse>> GetAllAsync(Guid empresaClienteId, CancellationToken cancellationToken = default)
    {
        var regras = await _regraRepository.GetAllAsync(cancellationToken);
        return regras.Select(MapToResponse);
    }

    public async Task<IEnumerable<RegraCobrancaResponse>> GetRegrasAtivasAsync(Guid empresaClienteId, CancellationToken cancellationToken = default)
    {
        var regras = await _regraRepository.GetAllAsync(cancellationToken);
        return regras.Where(r => r.Ativa).Select(MapToResponse);
    }

    public async Task DeleteAsync(Guid empresaClienteId, Guid id, CancellationToken cancellationToken = default)
    {
        var regra = await _regraRepository.SingleOrDefaultAsync(r => r.Id == id && r.EmpresaClienteId == empresaClienteId, cancellationToken);

        if (regra == null)
            throw new KeyNotFoundException($"Regra de cobrança com ID {id} não encontrada");

        if (regra.EhPadrao)
            throw new InvalidOperationException("A regra padrão 'Envio Imediato' não pode ser deletada");

        _regraRepository.Remove(regra);
        await _unitOfWork.CommitAsync(cancellationToken);
    }

    public async Task<RegraCobrancaResponse> AtivarAsync(Guid empresaClienteId, Guid id, CancellationToken cancellationToken = default)
    {
        var regra = await _regraRepository.SingleOrDefaultAsync(r => r.Id == id && r.EmpresaClienteId == empresaClienteId, cancellationToken);

        if (regra == null)
            throw new KeyNotFoundException($"Regra de cobrança com ID {id} não encontrada");

        regra.Ativar();
        _regraRepository.Update(regra);
        await _unitOfWork.CommitAsync(cancellationToken);

        return MapToResponse(regra);
    }

    public async Task<RegraCobrancaResponse> DesativarAsync(Guid empresaClienteId, Guid id, CancellationToken cancellationToken = default)
    {
        var regra = await _regraRepository.SingleOrDefaultAsync(r => r.Id == id && r.EmpresaClienteId == empresaClienteId, cancellationToken);

        if (regra == null)
            throw new KeyNotFoundException($"Regra de cobrança com ID {id} não encontrada");

        regra.Desativar();
        _regraRepository.Update(regra);
        await _unitOfWork.CommitAsync(cancellationToken);

        return MapToResponse(regra);
    }

    public async Task<RegraCobrancaResponse> RegenerarTokenAsync(Guid empresaClienteId, Guid id, CancellationToken cancellationToken = default)
    {
        var regra = await _regraRepository.SingleOrDefaultAsync(r => r.Id == id && r.EmpresaClienteId == empresaClienteId, cancellationToken);

        if (regra == null)
            throw new KeyNotFoundException($"Regra de cobrança com ID {id} não encontrada");

        regra.RegenerarToken();
        _regraRepository.Update(regra);
        await _unitOfWork.CommitAsync(cancellationToken);

        return MapToResponse(regra);
    }

    private RegraCobrancaResponse MapToResponse(RegraCobranca regra)
    {
        return new RegraCobrancaResponse
        {
            Id = regra.Id,
            EmpresaClienteId = regra.EmpresaClienteId,
            Nome = regra.Nome,
            Descricao = regra.Descricao,
            Ativa = regra.Ativa,
            EhPadrao = regra.EhPadrao,
            TipoMomento = regra.TipoMomento,
            ValorTempo = regra.ValorTempo,
            UnidadeTempo = regra.UnidadeTempo,
            CanalNotificacao = regra.CanalNotificacao,
            TemplateNotificacao = regra.TemplateNotificacao,
            VariaveisObrigatorias = regra.GetVariaveisObrigatorias(),
            VariaveisObrigatoriasSistema = regra.VariaveisObrigatoriasSistema,
            TokenWebhook = regra.TokenWebhook,
            CriadoEm = regra.CriadoEm,
            AtualizadoEm = regra.AtualizadoEm
        };
    }
}
