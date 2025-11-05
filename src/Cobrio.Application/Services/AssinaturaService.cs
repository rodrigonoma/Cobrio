using Cobrio.Application.DTOs.Assinatura;
using Cobrio.Application.Interfaces;
using Cobrio.Domain.Entities;
using Cobrio.Domain.Interfaces;
using Cobrio.Domain.ValueObjects;
using Microsoft.AspNetCore.Http;

namespace Cobrio.Application.Services;

public class AssinaturaService : IAssinaturaService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly Guid _empresaClienteId;

    public AssinaturaService(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
    {
        _unitOfWork = unitOfWork;
        _httpContextAccessor = httpContextAccessor;

        var tenantId = _httpContextAccessor.HttpContext?.Items["TenantId"];
        if (tenantId == null || tenantId is not Guid)
            throw new UnauthorizedAccessException("TenantId não encontrado no contexto");

        _empresaClienteId = (Guid)tenantId;
    }

    public async Task<AssinaturaResponse> CriarAssinaturaAsync(CreateAssinaturaRequest request, CancellationToken cancellationToken = default)
    {
        var plano = await _unitOfWork.PlanosOferta.GetByIdAsync(request.PlanoOfertaId, cancellationToken);
        if (plano == null)
            throw new KeyNotFoundException($"Plano de oferta com ID {request.PlanoOfertaId} não encontrado");

        var email = new Email(request.Email);
        var endereco = request.Logradouro != null
            ? new Endereco(
                logradouro: request.Logradouro,
                numero: request.Numero ?? string.Empty,
                bairro: request.Bairro ?? string.Empty,
                cidade: request.Cidade ?? string.Empty,
                estado: request.Estado ?? string.Empty,
                cep: request.CEP ?? string.Empty,
                pais: request.Pais ?? "Brasil",
                complemento: request.Complemento
              )
            : null;

        var assinante = new Assinante(
            empresaClienteId: _empresaClienteId,
            planoOfertaId: plano.Id,
            nome: request.Nome,
            email: email,
            diaVencimento: request.DataInicio?.Day ?? DateTime.UtcNow.Day,
            cpfCnpj: request.CpfCnpj,
            telefone: request.Telefone,
            endereco: endereco,
            iniciarTrial: request.IniciarEmTrial && plano.PeriodoTrialDias > 0,
            diasTrial: plano.PeriodoTrialDias
        );

        await _unitOfWork.Assinantes.AddAsync(assinante, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return MapearParaResponse(assinante, plano);
    }

    public async Task<AssinaturaResponse> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var assinante = await _unitOfWork.Assinantes.GetByIdComPlanoAsync(id, _empresaClienteId, cancellationToken);
        if (assinante == null)
            throw new KeyNotFoundException($"Assinatura com ID {id} não encontrada");

        return MapearParaResponse(assinante, assinante.PlanoOferta);
    }

    public async Task<IEnumerable<AssinaturaResponse>> ListarAsync(int pagina = 1, int tamanhoPagina = 50, CancellationToken cancellationToken = default)
    {
        var assinantes = await _unitOfWork.Assinantes.GetPorEmpresaAsync(_empresaClienteId, cancellationToken);

        return assinantes
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .Select(a => MapearParaResponse(a, a.PlanoOferta));
    }

    public async Task<AssinaturaResponse> AtualizarAsync(Guid id, UpdateAssinaturaRequest request, CancellationToken cancellationToken = default)
    {
        var assinante = await _unitOfWork.Assinantes.GetByIdComPlanoAsync(id, _empresaClienteId, cancellationToken);
        if (assinante == null)
            throw new KeyNotFoundException($"Assinatura com ID {id} não encontrada");

        Email? email = null;
        Endereco? endereco = null;

        if (request.Logradouro != null)
        {
            endereco = new Endereco(
                logradouro: request.Logradouro,
                numero: request.Numero ?? string.Empty,
                bairro: request.Bairro ?? string.Empty,
                cidade: request.Cidade ?? string.Empty,
                estado: request.Estado ?? string.Empty,
                cep: request.CEP ?? string.Empty,
                pais: request.Pais ?? "Brasil",
                complemento: request.Complemento
            );
        }

        assinante.AtualizarDadosPessoais(null, email, request.Telefone, endereco);

        _unitOfWork.Assinantes.Update(assinante);
        await _unitOfWork.CommitAsync(cancellationToken);

        return MapearParaResponse(assinante, assinante.PlanoOferta);
    }

    public async Task<AssinaturaResponse> CancelarAsync(Guid id, CancelarAssinaturaRequest request, CancellationToken cancellationToken = default)
    {
        var assinante = await _unitOfWork.Assinantes.GetByIdComPlanoAsync(id, _empresaClienteId, cancellationToken);
        if (assinante == null)
            throw new KeyNotFoundException($"Assinatura com ID {id} não encontrada");

        assinante.Cancelar(request.Motivo);

        _unitOfWork.Assinantes.Update(assinante);
        await _unitOfWork.CommitAsync(cancellationToken);

        return MapearParaResponse(assinante, assinante.PlanoOferta);
    }

    public async Task<AssinaturaResponse> SuspenderAsync(Guid id, string? motivo = null, CancellationToken cancellationToken = default)
    {
        var assinante = await _unitOfWork.Assinantes.GetByIdComPlanoAsync(id, _empresaClienteId, cancellationToken);
        if (assinante == null)
            throw new KeyNotFoundException($"Assinatura com ID {id} não encontrada");

        assinante.Suspender();

        _unitOfWork.Assinantes.Update(assinante);
        await _unitOfWork.CommitAsync(cancellationToken);

        return MapearParaResponse(assinante, assinante.PlanoOferta);
    }

    public async Task<AssinaturaResponse> ReativarAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var assinante = await _unitOfWork.Assinantes.GetByIdComPlanoAsync(id, _empresaClienteId, cancellationToken);
        if (assinante == null)
            throw new KeyNotFoundException($"Assinatura com ID {id} não encontrada");

        assinante.Reativar();

        _unitOfWork.Assinantes.Update(assinante);
        await _unitOfWork.CommitAsync(cancellationToken);

        return MapearParaResponse(assinante, assinante.PlanoOferta);
    }

    public async Task<AssinaturaResponse> AlterarPlanoAsync(Guid id, Guid novoPlanoId, CancellationToken cancellationToken = default)
    {
        var assinante = await _unitOfWork.Assinantes.GetByIdComPlanoAsync(id, _empresaClienteId, cancellationToken);
        if (assinante == null)
            throw new KeyNotFoundException($"Assinatura com ID {id} não encontrada");

        var novoPlano = await _unitOfWork.PlanosOferta.GetByIdAsync(novoPlanoId, cancellationToken);
        if (novoPlano == null)
            throw new KeyNotFoundException($"Plano de oferta com ID {novoPlanoId} não encontrado");

        assinante.MudarPlano(novoPlanoId);

        _unitOfWork.Assinantes.Update(assinante);
        await _unitOfWork.CommitAsync(cancellationToken);

        return MapearParaResponse(assinante, novoPlano);
    }

    private static AssinaturaResponse MapearParaResponse(Assinante assinante, PlanoOferta plano)
    {
        return new AssinaturaResponse
        {
            Id = assinante.Id,
            EmpresaClienteId = assinante.EmpresaClienteId,
            AssinanteId = assinante.Id,
            PlanoOfertaId = assinante.PlanoOfertaId,
            PlanoNome = plano?.Nome ?? string.Empty,
            AssinanteNome = assinante.Nome,
            AssinanteEmail = assinante.Email.Endereco,
            Status = assinante.Status.ToString(),
            Valor = plano?.Valor.Valor ?? 0,
            TipoCiclo = plano?.TipoCiclo.ToString() ?? string.Empty,
            DataInicio = assinante.DataInicio,
            DataProximaCobranca = assinante.ProximaCobranca,
            DataCancelamento = assinante.DataCancelamento,
            DataSuspensao = null,
            DataExpiracao = assinante.DataFimCiclo,
            TrialInicio = assinante.EmTrial ? assinante.DataInicio : null,
            TrialFim = assinante.DataFimTrial,
            EmTrial = assinante.EmTrial,
            CriadoEm = assinante.CriadoEm,
            AtualizadoEm = assinante.AtualizadoEm
        };
    }
}
