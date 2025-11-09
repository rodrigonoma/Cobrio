using System.Text.Json;
using System.Text.RegularExpressions;
using Cobrio.Domain.Enums;

namespace Cobrio.Domain.Entities;

public class TemplateEmail : BaseEntity
{
    public Guid EmpresaClienteId { get; private set; }

    // Informações do template
    public string Nome { get; private set; }
    public string? Descricao { get; private set; }

    // Conteúdo do template
    public string ConteudoHtml { get; private set; }

    // Subject do email (opcional)
    public string? SubjectEmail { get; private set; }

    // Lista de variáveis obrigatórias extraídas do template (JSON array)
    public string VariaveisObrigatorias { get; private set; }

    // Lista de variáveis obrigatórias do sistema (JSON array) - ex: Email, Nome
    public string? VariaveisObrigatoriasSistema { get; private set; }

    // Canal sugerido (opcional, para contexto)
    public CanalNotificacao? CanalSugerido { get; private set; }

    // Navegação
    public EmpresaCliente EmpresaCliente { get; private set; } = null!;

    // Construtor para EF Core
    private TemplateEmail() { }

    public TemplateEmail(
        Guid empresaClienteId,
        string nome,
        string conteudoHtml,
        string? descricao = null,
        string? subjectEmail = null,
        List<string>? variaveisObrigatoriasSistema = null,
        CanalNotificacao? canalSugerido = null)
    {
        if (empresaClienteId == Guid.Empty)
            throw new ArgumentException("EmpresaClienteId inválido", nameof(empresaClienteId));

        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome não pode ser vazio", nameof(nome));

        if (string.IsNullOrWhiteSpace(conteudoHtml))
            throw new ArgumentException("Conteúdo HTML não pode ser vazio", nameof(conteudoHtml));

        EmpresaClienteId = empresaClienteId;
        Nome = nome.Trim();
        Descricao = descricao?.Trim();
        ConteudoHtml = conteudoHtml.Trim();
        SubjectEmail = string.IsNullOrWhiteSpace(subjectEmail) ? null : subjectEmail.Trim();
        VariaveisObrigatorias = JsonSerializer.Serialize(ExtrairVariaveis(conteudoHtml));
        VariaveisObrigatoriasSistema = variaveisObrigatoriasSistema != null && variaveisObrigatoriasSistema.Any()
            ? JsonSerializer.Serialize(variaveisObrigatoriasSistema)
            : null;
        CanalSugerido = canalSugerido;
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
                var variavelCompleta = match.Groups[1].Value.Trim();
                var variavelLimpa = LimparHtmlVariavel(variavelCompleta);
                variaveis.Add(variavelLimpa);
            }
        }

        return variaveis.ToList();
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

    public List<string> GetVariaveisObrigatorias()
    {
        return JsonSerializer.Deserialize<List<string>>(VariaveisObrigatorias) ?? new List<string>();
    }

    public List<string> GetVariaveisObrigatoriasSistema()
    {
        if (string.IsNullOrWhiteSpace(VariaveisObrigatoriasSistema))
            return new List<string>();

        return JsonSerializer.Deserialize<List<string>>(VariaveisObrigatoriasSistema) ?? new List<string>();
    }

    public void Atualizar(
        string? nome = null,
        string? descricao = null,
        string? conteudoHtml = null,
        string? subjectEmail = null,
        List<string>? variaveisObrigatoriasSistema = null,
        CanalNotificacao? canalSugerido = null)
    {
        if (!string.IsNullOrWhiteSpace(nome))
            Nome = nome.Trim();

        if (descricao != null)
            Descricao = string.IsNullOrWhiteSpace(descricao) ? null : descricao.Trim();

        if (!string.IsNullOrWhiteSpace(conteudoHtml))
        {
            ConteudoHtml = conteudoHtml.Trim();
            VariaveisObrigatorias = JsonSerializer.Serialize(ExtrairVariaveis(conteudoHtml));
        }

        if (subjectEmail != null)
            SubjectEmail = string.IsNullOrWhiteSpace(subjectEmail) ? null : subjectEmail.Trim();

        if (variaveisObrigatoriasSistema != null)
        {
            VariaveisObrigatoriasSistema = variaveisObrigatoriasSistema.Any()
                ? JsonSerializer.Serialize(variaveisObrigatoriasSistema)
                : null;
        }

        if (canalSugerido.HasValue)
            CanalSugerido = canalSugerido.Value;

        AtualizarDataModificacao();
    }
}
