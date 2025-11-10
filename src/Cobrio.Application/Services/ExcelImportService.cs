using Cobrio.Application.DTOs.RegraCobranca;
using Cobrio.Application.Interfaces;
using Cobrio.Domain.Entities;
using Cobrio.Domain.Enums;
using Cobrio.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Globalization;

namespace Cobrio.Application.Services;

public class ExcelImportService
{
    private readonly ILogger<ExcelImportService> _logger;
    private readonly IRegraCobrancaRepository _regraRepository;
    private readonly ICobrancaRepository _cobrancaRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHistoricoImportacaoRepository _historicoRepository;

    public ExcelImportService(
        ILogger<ExcelImportService> logger,
        IRegraCobrancaRepository regraRepository,
        ICobrancaRepository cobrancaRepository,
        IUnitOfWork unitOfWork,
        IHistoricoImportacaoRepository historicoRepository)
    {
        _logger = logger;
        _regraRepository = regraRepository;
        _cobrancaRepository = cobrancaRepository;
        _unitOfWork = unitOfWork;
        _historicoRepository = historicoRepository;
    }

    public async Task<byte[]> GerarModeloExcelAsync(Guid regraCobrancaId, CancellationToken cancellationToken = default)
    {
        var regra = await _regraRepository.GetByIdAsync(regraCobrancaId, cancellationToken);
        if (regra == null)
        {
            throw new InvalidOperationException("Regra de cobrança não encontrada");
        }

        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Modelo");

        // Criar cabeçalhos
        var col = 1;

        // Campos obrigatórios do sistema
        var camposSistema = new List<string> { "Email", "DataVencimento" };
        if (regra.CanalNotificacao == CanalNotificacao.SMS || regra.CanalNotificacao == CanalNotificacao.WhatsApp)
        {
            camposSistema = new List<string> { "Telefone", "DataVencimento" };
        }

        foreach (var campo in camposSistema)
        {
            worksheet.Cells[1, col].Value = campo;
            worksheet.Cells[1, col].Style.Font.Bold = true;
            worksheet.Cells[1, col].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            worksheet.Cells[1, col].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
            col++;
        }

        // Variáveis personalizadas (excluindo as que já são campos do sistema)
        if (!string.IsNullOrEmpty(regra.VariaveisObrigatorias))
        {
            var variaveis = JsonSerializer.Deserialize<List<string>>(regra.VariaveisObrigatorias);
            if (variaveis != null)
            {
                // Filtrar variáveis que já existem nos campos do sistema (comparação case-insensitive)
                var variaveisFiltradas = variaveis
                    .Where(v => !camposSistema.Any(c => string.Equals(c, LimparHtml(v), StringComparison.OrdinalIgnoreCase)))
                    .ToList();

                foreach (var variavel in variaveisFiltradas)
                {
                    var variavelLimpa = LimparHtml(variavel);
                    worksheet.Cells[1, col].Value = variavelLimpa;
                    worksheet.Cells[1, col].Style.Font.Bold = true;
                    worksheet.Cells[1, col].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    worksheet.Cells[1, col].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGreen);
                    col++;
                }
            }
        }

        // Adicionar linha de exemplo
        col = 1;
        if (regra.CanalNotificacao == CanalNotificacao.Email)
        {
            worksheet.Cells[2, col++].Value = "exemplo@email.com";
        }
        else
        {
            worksheet.Cells[2, col++].Value = "+5511999999999";
        }

        // Formato da data depende da unidade de tempo
        var dataExemplo = DateTime.Now.AddDays(7);
        if (regra.UnidadeTempo == UnidadeTempo.Minutos || regra.UnidadeTempo == UnidadeTempo.Horas)
        {
            worksheet.Cells[2, col++].Value = dataExemplo.ToString("yyyy-MM-dd HH:mm:ss");
        }
        else
        {
            worksheet.Cells[2, col++].Value = dataExemplo.ToString("yyyy-MM-dd");
        }

        if (!string.IsNullOrEmpty(regra.VariaveisObrigatorias))
        {
            var variaveis = JsonSerializer.Deserialize<List<string>>(regra.VariaveisObrigatorias);
            if (variaveis != null)
            {
                // Filtrar variáveis que já existem nos campos do sistema
                var variaveisFiltradas = variaveis
                    .Where(v => !camposSistema.Any(c => string.Equals(c, LimparHtml(v), StringComparison.OrdinalIgnoreCase)))
                    .ToList();

                foreach (var variavel in variaveisFiltradas)
                {
                    var variavelLimpa = LimparHtml(variavel);
                    worksheet.Cells[2, col++].Value = $"Exemplo {variavelLimpa}";
                }
            }
        }

        // Auto-ajustar colunas
        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

        return package.GetAsByteArray();
    }

    public async Task<ImportacaoResultado> ImportarCobrancasAsync(
        Guid regraCobrancaId,
        Stream excelStream,
        string nomeArquivo,
        bool envioImediato = false,
        CancellationToken cancellationToken = default)
    {
        var resultado = new ImportacaoResultado();

        var regra = await _regraRepository.GetByIdAsync(regraCobrancaId, cancellationToken);
        if (regra == null)
        {
            resultado.Sucesso = false;
            resultado.Mensagem = "Regra de cobrança não encontrada";
            return resultado;
        }

        using var package = new ExcelPackage(excelStream);
        var worksheet = package.Workbook.Worksheets.First();

        if (worksheet.Dimension == null)
        {
            resultado.Sucesso = false;
            resultado.Mensagem = "Planilha vazia";
            return resultado;
        }

        // Ler cabeçalhos
        var colunas = new Dictionary<string, int>();
        for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
        {
            var nomeColuna = worksheet.Cells[1, col].Value?.ToString();
            if (!string.IsNullOrEmpty(nomeColuna))
            {
                colunas[nomeColuna] = col;
            }
        }

        // Validar campos obrigatórios
        var campoDestinatario = regra.CanalNotificacao == CanalNotificacao.Email ? "Email" : "Telefone";
        if (!colunas.ContainsKey(campoDestinatario) || !colunas.ContainsKey("DataVencimento"))
        {
            resultado.Sucesso = false;
            resultado.Mensagem = $"Campos obrigatórios ausentes: {campoDestinatario}, DataVencimento";
            return resultado;
        }

        // Processar linhas
        var cobrancas = new List<Cobranca>();
        for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
        {
            try
            {
                var destinatario = worksheet.Cells[row, colunas[campoDestinatario]].Value?.ToString();
                var dataVencimentoStr = worksheet.Cells[row, colunas["DataVencimento"]].Value?.ToString();

                // Validar campos obrigatórios vazios
                if (string.IsNullOrEmpty(destinatario) || string.IsNullOrEmpty(dataVencimentoStr))
                {
                    resultado.LinhasComErro++;
                    resultado.Erros.Add(new ErroValidacaoLinha
                    {
                        NumeroLinha = row,
                        TipoErro = "Campos Obrigatórios",
                        Descricao = $"Os campos '{campoDestinatario}' e 'DataVencimento' são obrigatórios e não podem estar vazios",
                        ValorInvalido = string.IsNullOrEmpty(destinatario) ? $"{campoDestinatario}: vazio" : "DataVencimento: vazio"
                    });
                    _logger.LogWarning("Linha {Row} ignorada: campos obrigatórios vazios", row);
                    continue;
                }

                // Validar email se for canal de email
                if (regra.CanalNotificacao == CanalNotificacao.Email)
                {
                    if (!IsValidEmail(destinatario))
                    {
                        resultado.LinhasComErro++;
                        resultado.Erros.Add(new ErroValidacaoLinha
                        {
                            NumeroLinha = row,
                            TipoErro = "Email Inválido",
                            Descricao = "O endereço de email fornecido não é válido",
                            ValorInvalido = destinatario
                        });
                        _logger.LogWarning("Linha {Row} ignorada: email inválido", row);
                        continue;
                    }
                }

                // Validar telefone se for SMS ou WhatsApp
                if (regra.CanalNotificacao == CanalNotificacao.SMS || regra.CanalNotificacao == CanalNotificacao.WhatsApp)
                {
                    if (!IsValidPhone(destinatario))
                    {
                        resultado.LinhasComErro++;
                        resultado.Erros.Add(new ErroValidacaoLinha
                        {
                            NumeroLinha = row,
                            TipoErro = "Telefone Inválido",
                            Descricao = "O número de telefone deve conter apenas dígitos e opcionalmente o símbolo '+'",
                            ValorInvalido = destinatario
                        });
                        _logger.LogWarning("Linha {Row} ignorada: telefone inválido", row);
                        continue;
                    }
                }

                // Validar data de vencimento
                if (!TryParseDataVencimento(dataVencimentoStr, out var dataVencimento))
                {
                    resultado.LinhasComErro++;
                    resultado.Erros.Add(new ErroValidacaoLinha
                    {
                        NumeroLinha = row,
                        TipoErro = "Data Inválida",
                        Descricao = "A data de vencimento não está em um formato válido. Formatos aceitos: dd/MM/yyyy HH:mm, yyyy-MM-dd HH:mm:ss, etc.",
                        ValorInvalido = dataVencimentoStr
                    });
                    _logger.LogWarning("Linha {Row} ignorada: data de vencimento inválida", row);
                    continue;
                }

                // Construir payload com as variáveis
                var payload = new Dictionary<string, object>();
                payload[campoDestinatario] = destinatario;

                // Adicionar DataVencimento ao payload para substituição no template
                if (regra.UnidadeTempo == UnidadeTempo.Minutos || regra.UnidadeTempo == UnidadeTempo.Horas)
                {
                    payload["dataVencimento"] = dataVencimento.ToString("yyyy-MM-dd HH:mm:ss");
                }
                else
                {
                    payload["dataVencimento"] = dataVencimento.ToString("yyyy-MM-dd");
                }

                // Adicionar variáveis personalizadas
                if (!string.IsNullOrEmpty(regra.VariaveisObrigatorias))
                {
                    var variaveis = JsonSerializer.Deserialize<List<string>>(regra.VariaveisObrigatorias);
                    if (variaveis != null)
                    {
                        foreach (var variavel in variaveis)
                        {
                            var variavelLimpa = LimparHtml(variavel);

                            // Busca a coluna pelo nome limpo (sem HTML)
                            if (colunas.ContainsKey(variavelLimpa))
                            {
                                var valor = worksheet.Cells[row, colunas[variavelLimpa]].Value?.ToString();
                                if (!string.IsNullOrEmpty(valor))
                                {
                                    // Adiciona ao payload com o nome limpo
                                    payload[variavelLimpa] = valor;
                                }
                            }
                        }
                    }
                }

                try
                {
                    var cobranca = new Cobranca(
                        regraCobrancaId,
                        regra.EmpresaClienteId,
                        JsonSerializer.Serialize(payload),
                        dataVencimento,
                        regra.TipoMomento,
                        regra.ValorTempo,
                        regra.UnidadeTempo,
                        regra.EhPadrao);

                    cobrancas.Add(cobranca);
                    resultado.LinhasProcessadas++;
                }
                catch (ArgumentException argEx)
                {
                    // Captura erros de validação da entidade Cobranca (ex: data de disparo no passado)
                    resultado.LinhasComErro++;
                    resultado.Erros.Add(new ErroValidacaoLinha
                    {
                        NumeroLinha = row,
                        TipoErro = "Data Vencida",
                        Descricao = argEx.Message,
                        ValorInvalido = dataVencimentoStr
                    });
                    _logger.LogWarning("Linha {Row} ignorada: {Message}", row, argEx.Message);
                }
            }
            catch (Exception ex)
            {
                resultado.LinhasComErro++;
                resultado.Erros.Add(new ErroValidacaoLinha
                {
                    NumeroLinha = row,
                    TipoErro = "Erro Desconhecido",
                    Descricao = ex.Message,
                    ValorInvalido = null
                });
                _logger.LogError(ex, "Erro ao processar linha {Row}", row);
            }
        }

        // Salvar cobranças
        if (cobrancas.Any())
        {
            foreach (var cobranca in cobrancas)
            {
                await _cobrancaRepository.AddAsync(cobranca, cancellationToken);
            }
            await _unitOfWork.CommitAsync(cancellationToken);

            resultado.Sucesso = true;
            resultado.Mensagem = $"{resultado.LinhasProcessadas} cobranças criadas com sucesso";
        }
        else
        {
            resultado.Sucesso = false;
            resultado.Mensagem = "Nenhuma cobrança válida encontrada no arquivo";
        }

        // Salvar histórico da importação
        var totalLinhas = worksheet.Dimension.End.Row - 1; // Excluir cabeçalho
        var statusImportacao = resultado.Sucesso
            ? (resultado.LinhasComErro > 0 ? StatusImportacao.Parcial : StatusImportacao.Sucesso)
            : StatusImportacao.Erro;

        var errosJson = resultado.Erros.Any()
            ? JsonSerializer.Serialize(resultado.Erros)
            : null;

        var historico = new HistoricoImportacao(
            regraCobrancaId,
            regra.EmpresaClienteId,
            null, // UsuarioId - pode ser obtido do contexto HTTP se disponível
            nomeArquivo,
            totalLinhas,
            resultado.LinhasProcessadas,
            resultado.LinhasComErro,
            statusImportacao,
            OrigemImportacao.Excel,
            errosJson);

        await _historicoRepository.AddAsync(historico, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return resultado;
    }

    private string LimparHtml(string texto)
    {
        if (string.IsNullOrEmpty(texto))
            return texto;

        // Remove tags HTML
        var semHtml = Regex.Replace(texto, @"<\/?[^>]+(>|$)", "");

        // Remove entidades HTML
        semHtml = Regex.Replace(semHtml, @"&nbsp;", " ");
        semHtml = Regex.Replace(semHtml, @"&[a-z]+;", "", RegexOptions.IgnoreCase);

        // Normaliza espaços múltiplos
        semHtml = Regex.Replace(semHtml, @"\s+", " ");

        return semHtml.Trim();
    }

    private bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            // Validação básica de email usando regex
            var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, emailPattern, RegexOptions.IgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    private bool IsValidPhone(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return false;

        // Remove espaços, hífens e parênteses para validação
        var cleanPhone = Regex.Replace(phone, @"[\s\-\(\)]", "");

        // Telefone deve ter apenas dígitos e opcionalmente começar com +
        var phonePattern = @"^\+?\d{10,15}$";
        return Regex.IsMatch(cleanPhone, phonePattern);
    }

    private bool TryParseDataVencimento(string dataStr, out DateTime data)
    {
        data = DateTime.MinValue;

        if (string.IsNullOrWhiteSpace(dataStr))
            return false;

        // Formatos aceitos
        string[] formatos = new[]
        {
            "yyyy-MM-dd HH:mm:ss",
            "yyyy-MM-dd HH:mm",
            "yyyy-MM-dd",
            "dd/MM/yyyy HH:mm:ss",
            "dd/MM/yyyy HH:mm",
            "dd/MM/yyyy",
            "MM/dd/yyyy HH:mm:ss",
            "MM/dd/yyyy HH:mm",
            "MM/dd/yyyy"
        };

        // Tentar parse com formatos específicos (cultura invariante)
        if (DateTime.TryParseExact(dataStr, formatos, CultureInfo.InvariantCulture, DateTimeStyles.None, out data))
            return true;

        // Tentar parse com cultura brasileira
        if (DateTime.TryParse(dataStr, new CultureInfo("pt-BR"), DateTimeStyles.None, out data))
            return true;

        // Tentar parse genérico
        if (DateTime.TryParse(dataStr, out data))
            return true;

        return false;
    }

    public async Task<ImportacaoResultado> ImportarCobrancasJsonAsync(
        Guid regraCobrancaId,
        List<DTOs.Cobranca.CreateCobrancaRequest> cobrancasRequest,
        bool envioImediato = false,
        CancellationToken cancellationToken = default)
    {
        var resultado = new ImportacaoResultado();
        var totalLinhas = cobrancasRequest.Count;

        var regra = await _regraRepository.GetByIdAsync(regraCobrancaId, cancellationToken);
        if (regra == null)
        {
            resultado.Sucesso = false;
            resultado.Mensagem = "Regra de cobrança não encontrada";
            return resultado;
        }

        var cobrancas = new List<Cobranca>();
        var numeroLinha = 0;

        foreach (var request in cobrancasRequest)
        {
            numeroLinha++;

            try
            {
                // Validar DataVencimento
                if (string.IsNullOrWhiteSpace(request.DataVencimento))
                {
                    resultado.LinhasComErro++;
                    resultado.Erros.Add(new ErroValidacaoLinha
                    {
                        NumeroLinha = numeroLinha,
                        TipoErro = "Campo Obrigatório",
                        Descricao = "Data de vencimento é obrigatória",
                        ValorInvalido = "DataVencimento: vazio"
                    });
                    continue;
                }

                if (!TryParseDataVencimento(request.DataVencimento, out var dataVencimento))
                {
                    resultado.LinhasComErro++;
                    resultado.Erros.Add(new ErroValidacaoLinha
                    {
                        NumeroLinha = numeroLinha,
                        TipoErro = "Data Inválida",
                        Descricao = $"Data de vencimento inválida: {request.DataVencimento}. Formatos aceitos: dd/MM/yyyy HH:mm, yyyy-MM-dd HH:mm:ss, etc.",
                        ValorInvalido = request.DataVencimento
                    });
                    continue;
                }

                // Validar destinatário conforme canal
                var destinatario = regra.CanalNotificacao == CanalNotificacao.Email ? request.Email : request.Telefone;
                if (string.IsNullOrEmpty(destinatario))
                {
                    resultado.LinhasComErro++;
                    resultado.Erros.Add(new ErroValidacaoLinha
                    {
                        NumeroLinha = numeroLinha,
                        TipoErro = "Campo Obrigatório",
                        Descricao = $"{(regra.CanalNotificacao == CanalNotificacao.Email ? "Email" : "Telefone")} é obrigatório",
                        ValorInvalido = request.NomeCliente ?? "N/A"
                    });
                    continue;
                }

                // Validar variáveis do template que estão no Payload
                // IMPORTANTE: Não validar dataVencimento porque ele vai na raiz do JSON como DataVencimento
                // e é adicionado automaticamente ao payload depois
                var variaveisObrigatoriasLimpas = regra.GetVariaveisObrigatoriasLimpas()
                    .Where(v => !v.Equals("dataVencimento", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                var variaveisFaltando = variaveisObrigatoriasLimpas
                    .Where(v => request.Payload == null || !request.Payload.ContainsKey(v))
                    .ToList();

                if (variaveisFaltando.Any())
                {
                    resultado.LinhasComErro++;
                    resultado.Erros.Add(new ErroValidacaoLinha
                    {
                        NumeroLinha = numeroLinha,
                        TipoErro = "Variáveis Faltando",
                        Descricao = $"Variáveis do template faltando: {string.Join(", ", variaveisFaltando)}",
                        ValorInvalido = request.NomeCliente ?? "N/A"
                    });
                    continue;
                }

                // Criar payload completo
                var payloadCompleto = new Dictionary<string, object>(request.Payload ?? new Dictionary<string, object>());

                // Adicionar destinatário ao payload
                var campoDestinatario = regra.CanalNotificacao == CanalNotificacao.Email ? "email" : "telefone";
                payloadCompleto[campoDestinatario] = destinatario;

                // Adicionar dataVencimento ao payload
                if (regra.UnidadeTempo == UnidadeTempo.Minutos || regra.UnidadeTempo == UnidadeTempo.Horas)
                {
                    payloadCompleto["dataVencimento"] = dataVencimento.ToString("yyyy-MM-dd HH:mm:ss");
                }
                else
                {
                    payloadCompleto["dataVencimento"] = dataVencimento.ToString("yyyy-MM-dd");
                }

                try
                {
                    // Criar cobrança (o construtor calcula a data de disparo internamente)
                    var cobranca = new Cobranca(
                        regraCobrancaId,
                        regra.EmpresaClienteId,
                        JsonSerializer.Serialize(payloadCompleto),
                        dataVencimento,
                        regra.TipoMomento,
                        regra.ValorTempo,
                        regra.UnidadeTempo,
                        regra.EhPadrao);

                    cobrancas.Add(cobranca);
                    resultado.LinhasProcessadas++;
                }
                catch (ArgumentException argEx)
                {
                    // Captura erros de validação da entidade Cobranca (ex: data de disparo no passado)
                    resultado.LinhasComErro++;
                    resultado.Erros.Add(new ErroValidacaoLinha
                    {
                        NumeroLinha = numeroLinha,
                        TipoErro = "Data Vencida",
                        Descricao = argEx.Message,
                        ValorInvalido = request.DataVencimento
                    });
                    _logger.LogWarning("Linha {NumeroLinha} ignorada: {Message}", numeroLinha, argEx.Message);
                }
            }
            catch (Exception ex)
            {
                resultado.LinhasComErro++;
                resultado.Erros.Add(new ErroValidacaoLinha
                {
                    NumeroLinha = numeroLinha,
                    TipoErro = ex.GetType().Name,
                    Descricao = ex.Message,
                    ValorInvalido = request.NomeCliente ?? "N/A"
                });
                _logger.LogWarning(ex, "Erro ao processar linha {NumeroLinha} do JSON", numeroLinha);
            }
        }

        // Salvar cobranças
        if (cobrancas.Any())
        {
            foreach (var cobranca in cobrancas)
            {
                await _cobrancaRepository.AddAsync(cobranca, cancellationToken);
            }
        }

        // Salvar histórico
        var statusImportacao = resultado.LinhasProcessadas == totalLinhas
            ? StatusImportacao.Sucesso
            : resultado.LinhasProcessadas > 0
                ? StatusImportacao.Parcial
                : StatusImportacao.Erro;

        var errosJson = resultado.Erros.Any()
            ? JsonSerializer.Serialize(resultado.Erros)
            : null;

        var historico = new HistoricoImportacao(
            regraCobrancaId,
            regra.EmpresaClienteId,
            null,
            $"importacao-json-{DateTime.Now:yyyyMMdd-HHmmss}.json",
            totalLinhas,
            resultado.LinhasProcessadas,
            resultado.LinhasComErro,
            statusImportacao,
            OrigemImportacao.Json,
            errosJson);

        await _historicoRepository.AddAsync(historico, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        resultado.Sucesso = resultado.LinhasProcessadas > 0;
        resultado.Mensagem = resultado.Sucesso
            ? $"{resultado.LinhasProcessadas} de {totalLinhas} cobrança(s) importada(s) com sucesso"
            : "Nenhuma cobrança foi importada devido a erros de validação";

        _logger.LogInformation(
            "Importação JSON concluída. Regra: {RegraId}, Total: {Total}, Processadas: {Processadas}, Erros: {Erros}",
            regraCobrancaId, totalLinhas, resultado.LinhasProcessadas, resultado.LinhasComErro);

        return resultado;
    }
}

public class ImportacaoResultado
{
    public bool Sucesso { get; set; }
    public string Mensagem { get; set; } = string.Empty;
    public int LinhasProcessadas { get; set; }
    public int LinhasComErro { get; set; }
    public List<ErroValidacaoLinha> Erros { get; set; } = new List<ErroValidacaoLinha>();
}

public class ErroValidacaoLinha
{
    public int NumeroLinha { get; set; }
    public string TipoErro { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string? ValorInvalido { get; set; }
}
