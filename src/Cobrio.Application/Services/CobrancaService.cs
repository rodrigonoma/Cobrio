using System.Text.Json;
using Cobrio.Application.DTOs.Cobranca;
using Cobrio.Domain.Entities;
using Cobrio.Domain.Interfaces;

namespace Cobrio.Application.Services;

public class CobrancaService
{
    private readonly ICobrancaRepository _cobrancaRepository;
    private readonly IRegraCobrancaRepository _regraRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CobrancaService(
        ICobrancaRepository cobrancaRepository,
        IRegraCobrancaRepository regraRepository,
        IUnitOfWork unitOfWork)
    {
        _cobrancaRepository = cobrancaRepository;
        _regraRepository = regraRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CobrancaResponse> CreateByTokenAsync(
        string token,
        CreateCobrancaRequest request,
        CancellationToken cancellationToken = default)
    {
        var regra = await _regraRepository.SingleOrDefaultAsync(r => r.TokenWebhook == token, cancellationToken);

        if (regra == null)
            throw new UnauthorizedAccessException("Token inválido");

        if (!regra.Ativa)
            throw new InvalidOperationException("Regra de cobrança está inativa");

        // DataVencimento é sempre obrigatório
        if (string.IsNullOrWhiteSpace(request.DataVencimento))
            throw new ArgumentException("Data de vencimento é obrigatória");

        // Fazer parse da data de vencimento (aceita múltiplos formatos como o Excel)
        if (!DateTime.TryParse(request.DataVencimento, out var dataVencimento))
            throw new ArgumentException($"Data de vencimento inválida: {request.DataVencimento}. Use formatos como: yyyy-MM-dd, yyyy-MM-dd HH:mm:ss, dd/MM/yyyy ou dd/MM/yyyy HH:mm");

        // Validar campos obrigatórios do sistema configurados na regra
        ValidarCamposObrigatorios(request, regra.GetVariaveisObrigatoriasSistema());

        // Validar variáveis obrigatórias do template (com HTML limpo para comparação)
        // IMPORTANTE: Não validar dataVencimento porque ele vai na raiz do JSON como DataVencimento
        // e é adicionado automaticamente ao payload depois
        var variaveisObrigatorias = regra.GetVariaveisObrigatoriasLimpas()
            .Where(v => !v.Equals("dataVencimento", StringComparison.OrdinalIgnoreCase))
            .ToList();
        var variaveisFaltando = variaveisObrigatorias.Where(v => !request.Payload.ContainsKey(v)).ToList();

        if (variaveisFaltando.Any())
            throw new ArgumentException($"Variáveis do template faltando: {string.Join(", ", variaveisFaltando)}");

        // Montar payload completo com campos obrigatórios do sistema + variáveis do template
        var payloadCompleto = new Dictionary<string, object>(request.Payload);

        // Adicionar campos obrigatórios do sistema ao payload
        if (!string.IsNullOrWhiteSpace(request.Email))
            payloadCompleto["Email"] = request.Email;

        if (!string.IsNullOrWhiteSpace(request.Telefone))
            payloadCompleto["Telefone"] = request.Telefone;

        if (!string.IsNullOrWhiteSpace(request.NomeCliente))
            payloadCompleto["NomeCliente"] = request.NomeCliente;

        var payloadJson = JsonSerializer.Serialize(payloadCompleto);

        var cobranca = new Cobranca(
            regra.Id,
            regra.EmpresaClienteId,
            payloadJson,
            dataVencimento,
            regra.TipoMomento,
            regra.ValorTempo,
            regra.UnidadeTempo,
            regra.EhPadrao
        );

        await _cobrancaRepository.AddAsync(cobranca, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return MapToResponse(cobranca, regra.Nome);
    }

    private void ValidarCamposObrigatorios(CreateCobrancaRequest request, List<string> variaveisObrigatorias)
    {
        if (variaveisObrigatorias == null || !variaveisObrigatorias.Any())
            return;

        var erros = new List<string>();

        foreach (var variavel in variaveisObrigatorias)
        {
            // DataVencimento é validado separadamente antes, então pode ser ignorado aqui
            if (variavel.ToLower() == "datavencimento")
                continue;

            var valorCampo = variavel.ToLower() switch
            {
                "nomecliente" => request.NomeCliente,
                "email" => request.Email,
                "telefone" => request.Telefone,
                _ => null
            };

            if (string.IsNullOrWhiteSpace(valorCampo))
            {
                erros.Add(variavel);
            }
            else
            {
                // Validações específicas de formato
                if (variavel.ToLower() == "email" && !valorCampo.Contains("@"))
                    throw new ArgumentException($"Campo '{variavel}' está em formato inválido (deve ser um email válido)");

                if (variavel.ToLower() == "telefone" && !valorCampo.StartsWith("+"))
                    throw new ArgumentException($"Campo '{variavel}' está em formato inválido (deve começar com + e incluir o código do país)");
            }
        }

        if (erros.Any())
            throw new ArgumentException($"Campos obrigatórios faltando: {string.Join(", ", erros)}");
    }

    public async Task<IEnumerable<CobrancaResponse>> GetByRegraIdAsync(
        Guid empresaClienteId,
        Guid regraId,
        CancellationToken cancellationToken = default)
    {
        var cobrancas = await _cobrancaRepository.GetByRegraIdAsync(regraId, cancellationToken);
        return cobrancas.Select(c => MapToResponse(c, string.Empty));
    }

    private CobrancaResponse MapToResponse(Cobranca cobranca, string regraCobrancaNome)
    {
        return new CobrancaResponse
        {
            Id = cobranca.Id,
            RegraCobrancaId = cobranca.RegraCobrancaId,
            RegraCobrancaNome = regraCobrancaNome,
            DataVencimento = cobranca.DataVencimento,
            DataDisparo = cobranca.DataDisparo,
            Status = cobranca.Status,
            TentativasEnvio = cobranca.TentativasEnvio,
            DataProcessamento = cobranca.DataProcessamento,
            MensagemErro = cobranca.MensagemErro,
            CriadoEm = cobranca.CriadoEm
        };
    }
}
