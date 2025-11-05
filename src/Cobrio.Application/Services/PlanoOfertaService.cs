using Cobrio.Application.DTOs.PlanoOferta;
using Cobrio.Application.Interfaces;
using Cobrio.Domain.Entities;
using Cobrio.Domain.Enums;
using Cobrio.Domain.Interfaces;
using Cobrio.Domain.ValueObjects;
using Microsoft.AspNetCore.Http;

namespace Cobrio.Application.Services;

public class PlanoOfertaService : IPlanoOfertaService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly Guid _empresaClienteId;

    public PlanoOfertaService(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
    {
        _unitOfWork = unitOfWork;
        _httpContextAccessor = httpContextAccessor;

        var tenantId = _httpContextAccessor.HttpContext?.Items["TenantId"];
        if (tenantId == null || tenantId is not Guid)
            throw new UnauthorizedAccessException("TenantId não encontrado no contexto");

        _empresaClienteId = (Guid)tenantId;
    }

    public async Task<PlanoOfertaResponse> CriarAsync(CreatePlanoOfertaRequest request, CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<TipoCiclo>(request.TipoCiclo, out var tipoCiclo))
            throw new ArgumentException($"Tipo de ciclo inválido: {request.TipoCiclo}");

        var plano = new PlanoOferta(
            empresaClienteId: _empresaClienteId,
            nome: request.Nome,
            tipoCiclo: tipoCiclo,
            valor: Money.FromDecimal(request.Valor),
            descricao: request.Descricao,
            periodoTrialDias: request.PeriodoTrial,
            limiteUsuarios: request.LimiteUsuarios,
            permiteDowngrade: request.PermiteDowngrade,
            permiteUpgrade: request.PermiteUpgrade
        );

        await _unitOfWork.PlanosOferta.AddAsync(plano, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return MapearParaResponse(plano);
    }

    public async Task<PlanoOfertaResponse> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var plano = await _unitOfWork.PlanosOferta.GetByIdAsync(id, cancellationToken);

        if (plano == null)
            throw new KeyNotFoundException($"Plano de oferta com ID {id} não encontrado");

        return MapearParaResponse(plano);
    }

    public async Task<IEnumerable<PlanoOfertaResponse>> ListarAsync(bool? apenasAtivos = null, CancellationToken cancellationToken = default)
    {
        var planos = await _unitOfWork.PlanosOferta.GetAllAsync(cancellationToken);

        if (apenasAtivos == true)
            planos = planos.Where(p => p.Ativo);

        return planos.Select(MapearParaResponse);
    }

    public async Task<PlanoOfertaResponse> AtualizarAsync(Guid id, UpdatePlanoOfertaRequest request, CancellationToken cancellationToken = default)
    {
        var plano = await _unitOfWork.PlanosOferta.GetByIdAsync(id, cancellationToken);

        if (plano == null)
            throw new KeyNotFoundException($"Plano de oferta com ID {id} não encontrado");

        if (!string.IsNullOrWhiteSpace(request.Nome))
            plano.AtualizarNome(request.Nome);

        if (!string.IsNullOrWhiteSpace(request.Descricao))
            plano.AtualizarDescricao(request.Descricao);

        if (request.Valor.HasValue)
            plano.AtualizarValor(Money.FromDecimal(request.Valor.Value));

        // Atualizar configurações se algum valor foi fornecido
        if (request.LimiteUsuarios.HasValue || request.PermiteUpgrade.HasValue || request.PermiteDowngrade.HasValue)
        {
            plano.AtualizarConfiguracoes(
                limiteUsuarios: request.LimiteUsuarios ?? plano.LimiteUsuarios,
                permiteDowngrade: request.PermiteDowngrade ?? plano.PermiteDowngrade,
                permiteUpgrade: request.PermiteUpgrade ?? plano.PermiteUpgrade
            );
        }

        _unitOfWork.PlanosOferta.Update(plano);
        await _unitOfWork.CommitAsync(cancellationToken);

        return MapearParaResponse(plano);
    }

    public async Task AtivarAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var plano = await _unitOfWork.PlanosOferta.GetByIdAsync(id, cancellationToken);

        if (plano == null)
            throw new KeyNotFoundException($"Plano de oferta com ID {id} não encontrado");

        plano.Ativar();

        _unitOfWork.PlanosOferta.Update(plano);
        await _unitOfWork.CommitAsync(cancellationToken);
    }

    public async Task DesativarAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var plano = await _unitOfWork.PlanosOferta.GetByIdAsync(id, cancellationToken);

        if (plano == null)
            throw new KeyNotFoundException($"Plano de oferta com ID {id} não encontrado");

        plano.Desativar();

        _unitOfWork.PlanosOferta.Update(plano);
        await _unitOfWork.CommitAsync(cancellationToken);
    }

    public async Task ExcluirAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var plano = await _unitOfWork.PlanosOferta.GetByIdAsync(id, cancellationToken);

        if (plano == null)
            throw new KeyNotFoundException($"Plano de oferta com ID {id} não encontrado");

        // Verificar se há assinaturas ativas vinculadas
        var assinantes = await _unitOfWork.Assinantes.GetPorEmpresaAsync(_empresaClienteId, cancellationToken);
        if (assinantes.Any(a => a.PlanoOfertaId == id && a.Status != StatusAssinatura.Cancelado))
            throw new InvalidOperationException("Não é possível excluir um plano com assinaturas ativas");

        _unitOfWork.PlanosOferta.Remove(plano);
        await _unitOfWork.CommitAsync(cancellationToken);
    }

    private static PlanoOfertaResponse MapearParaResponse(PlanoOferta plano)
    {
        return new PlanoOfertaResponse
        {
            Id = plano.Id,
            EmpresaClienteId = plano.EmpresaClienteId,
            Nome = plano.Nome,
            Descricao = plano.Descricao,
            Valor = plano.Valor.Valor,
            Moeda = plano.Valor.Moeda,
            TipoCiclo = plano.TipoCiclo.ToString(),
            PeriodoTrial = plano.PeriodoTrialDias,
            LimiteUsuarios = plano.LimiteUsuarios,
            Ativo = plano.Ativo,
            PermiteUpgrade = plano.PermiteUpgrade,
            PermiteDowngrade = plano.PermiteDowngrade,
            CriadoEm = plano.CriadoEm,
            AtualizadoEm = plano.AtualizadoEm
        };
    }
}
