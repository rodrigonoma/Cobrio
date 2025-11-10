using System.Text.Json;
using System.Text.RegularExpressions;
using Cobrio.Domain.Enums;
using Cobrio.Domain.Validators;

namespace Cobrio.Domain.Entities;

public class RegraCobranca : BaseEntity
{
    public Guid EmpresaClienteId { get; private set; }

    // Informações básicas
    public string Nome { get; private set; }
    public string? Descricao { get; private set; }
    public bool Ativa { get; private set; }
    public bool EhPadrao { get; private set; } // Regra padrão (Envio Imediato) não pode ser deletada

    // Configuração de quando disparar (flexível)
    public TipoMomento TipoMomento { get; private set; }
    public int ValorTempo { get; private set; }
    public UnidadeTempo UnidadeTempo { get; private set; }

    // Configuração do canal de notificação
    public CanalNotificacao CanalNotificacao { get; private set; }

    // Template da notificação com variáveis {{NomeVariavel}}
    public string TemplateNotificacao { get; private set; }

    // Subject do email (apenas para canal Email)
    public string? SubjectEmail { get; private set; }

    // Lista de variáveis obrigatórias extraídas do template (JSON array) - vão dentro de payload
    public string VariaveisObrigatorias { get; private set; }

    // Lista de variáveis obrigatórias do sistema (JSON array) - vão na raiz do JSON
    public string? VariaveisObrigatoriasSistema { get; private set; }

    // Token único para identificar a regra na URL pública
    public string TokenWebhook { get; private set; }

    // Navegação
    public EmpresaCliente EmpresaCliente { get; private set; } = null!;
    public ICollection<Cobranca> Cobrancas { get; private set; } = new List<Cobranca>();

    // Construtor para EF Core
    private RegraCobranca() { }

    public RegraCobranca(
        Guid empresaClienteId,
        string nome,
        TipoMomento tipoMomento,
        int valorTempo,
        UnidadeTempo unidadeTempo,
        CanalNotificacao canalNotificacao,
        string templateNotificacao,
        List<string>? variaveisObrigatoriasSistema = null,
        string? descricao = null,
        string? subjectEmail = null)
    {
        if (empresaClienteId == Guid.Empty)
            throw new ArgumentException("EmpresaClienteId inválido", nameof(empresaClienteId));

        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome não pode ser vazio", nameof(nome));

        if (valorTempo <= 0)
            throw new ArgumentException("Valor do tempo deve ser maior que zero", nameof(valorTempo));

        if (string.IsNullOrWhiteSpace(templateNotificacao))
            throw new ArgumentException("Template de notificação não pode ser vazio", nameof(templateNotificacao));

        EmpresaClienteId = empresaClienteId;
        Nome = nome.Trim();
        Descricao = descricao?.Trim();
        TipoMomento = tipoMomento;
        ValorTempo = valorTempo;
        UnidadeTempo = unidadeTempo;
        CanalNotificacao = canalNotificacao;
        TemplateNotificacao = templateNotificacao.Trim();
        SubjectEmail = string.IsNullOrWhiteSpace(subjectEmail) ? null : subjectEmail.Trim();

        // Extrair variáveis do template E do subject (se houver)
        var variaveisTemplate = ExtrairVariaveis(templateNotificacao);
        if (!string.IsNullOrWhiteSpace(subjectEmail))
        {
            var variaveisSubject = ExtrairVariaveis(subjectEmail);
            // Combinar e remover duplicatas
            variaveisTemplate = variaveisTemplate.Union(variaveisSubject).ToList();
        }
        VariaveisObrigatorias = JsonSerializer.Serialize(variaveisTemplate);

        VariaveisObrigatoriasSistema = variaveisObrigatoriasSistema != null && variaveisObrigatoriasSistema.Any()
            ? JsonSerializer.Serialize(variaveisObrigatoriasSistema)
            : null;
        Ativa = true;
        TokenWebhook = GerarTokenWebhook();
    }

    private string GerarTokenWebhook()
    {
        return $"{Guid.NewGuid():N}";
    }

    private List<string> ExtrairVariaveis(string template)
    {
        var regex = new Regex(@"\{\{([^}]+)\}\}");
        var matches = regex.Matches(template);
        var variaveis = new HashSet<string>();

        foreach (Match match in matches)
        {
            if (match.Groups.Count > 1)
            {
                variaveis.Add(match.Groups[1].Value.Trim());
            }
        }

        return variaveis.ToList();
    }

    public List<string> GetVariaveisObrigatorias()
    {
        return JsonSerializer.Deserialize<List<string>>(VariaveisObrigatorias) ?? new List<string>();
    }

    /// <summary>
    /// Retorna as variáveis obrigatórias com HTML removido (para validação em webhooks/API)
    /// </summary>
    public List<string> GetVariaveisObrigatoriasLimpas()
    {
        var variaveis = GetVariaveisObrigatorias();
        return variaveis.Select(v => LimparHtmlVariavel(v)).ToList();
    }

    public List<string> GetVariaveisObrigatoriasSistema()
    {
        if (string.IsNullOrWhiteSpace(VariaveisObrigatoriasSistema))
            return new List<string>();

        return JsonSerializer.Deserialize<List<string>>(VariaveisObrigatoriasSistema) ?? new List<string>();
    }

    public void Ativar()
    {
        Ativa = true;
        AtualizarDataModificacao();
    }

    public void Desativar()
    {
        Ativa = false;
        AtualizarDataModificacao();
    }

    public void Atualizar(
        string? nome = null,
        string? descricao = null,
        TipoMomento? tipoMomento = null,
        int? valorTempo = null,
        UnidadeTempo? unidadeTempo = null,
        CanalNotificacao? canalNotificacao = null,
        string? templateNotificacao = null,
        List<string>? variaveisObrigatoriasSistema = null,
        string? subjectEmail = null)
    {
        // Regras padrão não podem ter nome, descrição ou tempo alterados
        if (EhPadrao)
        {
            if (!string.IsNullOrWhiteSpace(nome) || descricao != null ||
                tipoMomento.HasValue || valorTempo.HasValue || unidadeTempo.HasValue)
            {
                throw new InvalidOperationException("A regra padrão não pode ter nome, descrição ou configurações de tempo alteradas. Apenas o canal pode ser modificado.");
            }
        }

        if (!string.IsNullOrWhiteSpace(nome))
            Nome = nome.Trim();

        if (descricao != null)
            Descricao = descricao.Trim();

        if (valorTempo.HasValue)
        {
            if (valorTempo.Value <= 0)
                throw new ArgumentException("Valor do tempo deve ser maior que zero");
            ValorTempo = valorTempo.Value;
        }

        if (tipoMomento.HasValue)
            TipoMomento = tipoMomento.Value;

        if (unidadeTempo.HasValue)
            UnidadeTempo = unidadeTempo.Value;

        if (canalNotificacao.HasValue)
            CanalNotificacao = canalNotificacao.Value;

        if (!string.IsNullOrWhiteSpace(templateNotificacao))
        {
            TemplateNotificacao = templateNotificacao.Trim();
        }

        if (subjectEmail != null)
        {
            SubjectEmail = string.IsNullOrWhiteSpace(subjectEmail) ? null : subjectEmail.Trim();
        }

        // Re-extrair variáveis se template ou subject foram atualizados
        if (!string.IsNullOrWhiteSpace(templateNotificacao) || subjectEmail != null)
        {
            var variaveisTemplate = ExtrairVariaveis(TemplateNotificacao);
            if (!string.IsNullOrWhiteSpace(SubjectEmail))
            {
                var variaveisSubject = ExtrairVariaveis(SubjectEmail);
                // Combinar e remover duplicatas
                variaveisTemplate = variaveisTemplate.Union(variaveisSubject).ToList();
            }
            VariaveisObrigatorias = JsonSerializer.Serialize(variaveisTemplate);
        }

        if (variaveisObrigatoriasSistema != null)
        {
            VariaveisObrigatoriasSistema = variaveisObrigatoriasSistema.Any()
                ? JsonSerializer.Serialize(variaveisObrigatoriasSistema)
                : null;
        }

        AtualizarDataModificacao();
    }

    public void RegenerarToken()
    {
        TokenWebhook = GerarTokenWebhook();
        AtualizarDataModificacao();
    }

    public string GetUrlWebhookCompleta(string baseUrl)
    {
        return $"{baseUrl}/api/webhook/{TokenWebhook}";
    }

    public string ProcessarTemplate(Dictionary<string, object> valores)
    {
        return ProcessarTemplate(valores, TemplateNotificacao);
    }

    /// <summary>
    /// Processa um template customizado (útil para processar o Subject do email separadamente)
    /// </summary>
    public string ProcessarTemplate(Dictionary<string, object> valores, string template)
    {
        var resultado = template;

        // Padrão para capturar todo o conteúdo dentro de {{}}
        var pattern = @"\{\{(.+?)\}\}";
        var matches = Regex.Matches(resultado, pattern);

        // Processar cada match para substituir preservando formatação HTML
        foreach (Match match in matches)
        {
            var conteudoCompleto = match.Groups[1].Value; // Ex: "<strong>valor</strong>"
            var variavelLimpa = LimparHtmlVariavel(conteudoCompleto); // "valor"

            if (valores.TryGetValue(variavelLimpa, out var valor))
            {
                var valorStr = valor?.ToString() ?? string.Empty;

                // Substitui apenas o texto da variável, mantendo as tags HTML ao redor
                // Ex: "<strong>valor</strong>" -> "<strong>250.00</strong>"
                var conteudoSubstituido = Regex.Replace(
                    conteudoCompleto,
                    @"\b" + Regex.Escape(variavelLimpa) + @"\b",
                    valorStr,
                    RegexOptions.IgnoreCase
                );

                // Substitui o match completo {{...}} pelo conteúdo processado
                resultado = resultado.Replace(match.Value, conteudoSubstituido);
            }
        }

        return resultado;
    }

    private string LimparHtmlVariavel(string texto)
    {
        if (string.IsNullOrEmpty(texto))
            return texto;

        // Remove tags HTML
        var semHtml = Regex.Replace(texto, @"<\/?[^>]+(>|$)", "");
        // Remove entidades HTML
        semHtml = Regex.Replace(semHtml, @"&nbsp;", " ");
        semHtml = Regex.Replace(semHtml, @"&[a-z]+;", "", RegexOptions.IgnoreCase);
        // Remove espaços extras
        semHtml = Regex.Replace(semHtml, @"\s+", " ");
        return semHtml.Trim();
    }

    /// <summary>
    /// Valida o assunto do email contra palavras e padrões de spam
    /// </summary>
    public SubjectEmailValidator.ValidationResult ValidarSubjectEmail()
    {
        if (CanalNotificacao != CanalNotificacao.Email)
        {
            return new SubjectEmailValidator.ValidationResult { IsValid = true };
        }

        if (string.IsNullOrWhiteSpace(SubjectEmail))
        {
            return new SubjectEmailValidator.ValidationResult
            {
                IsValid = false,
                Errors = { "O assunto do email é obrigatório quando o canal de notificação é Email" }
            };
        }

        return SubjectEmailValidator.Validate(SubjectEmail);
    }

    /// <summary>
    /// Cria uma regra padrão de "Envio Imediato" que não pode ser deletada
    /// </summary>
    public static RegraCobranca CriarRegraPadrao(Guid empresaClienteId, CanalNotificacao canal = CanalNotificacao.Email)
    {
        var regra = new RegraCobranca
        {
            EmpresaClienteId = empresaClienteId,
            Nome = "Envio Imediato",
            Descricao = "Regra padrão para envio imediato de cobranças sem considerar data de vencimento",
            Ativa = true,
            EhPadrao = true,
            TipoMomento = TipoMomento.Antes,
            ValorTempo = 1,
            UnidadeTempo = UnidadeTempo.Minutos,
            CanalNotificacao = canal,
            TemplateNotificacao = canal == CanalNotificacao.Email
                ? "Prezado(a) {{nome}},\n\nEste é um lembrete sobre a cobrança no valor de {{valor}}.\n\nAtenciosamente,\nEquipe de Cobrança"
                : "Olá {{nome}}! Lembrete sobre a cobrança de {{valor}}. Entre em contato para mais informações.",
            SubjectEmail = canal == CanalNotificacao.Email ? "Lembrete de Cobrança - {{valor}}" : null,
            TokenWebhook = $"{Guid.NewGuid():N}"
        };

        // Extrair variáveis do template
        var variaveisTemplate = regra.ExtrairVariaveis(regra.TemplateNotificacao);
        regra.VariaveisObrigatorias = JsonSerializer.Serialize(variaveisTemplate);

        // Definir campos obrigatórios do sistema (Email sempre é obrigatório para envio)
        var camposObrigatoriosSistema = new List<string> { "Email" };
        regra.VariaveisObrigatoriasSistema = JsonSerializer.Serialize(camposObrigatoriosSistema);

        return regra;
    }
}
