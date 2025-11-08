using System.Text.RegularExpressions;

namespace Cobrio.Domain.Validators;

/// <summary>
/// Validador de assunto de email para evitar palavras que podem ser consideradas spam
/// </summary>
public static class SubjectEmailValidator
{
    // Lista de palavras/frases que são comuns em spam e devem ser evitadas
    private static readonly string[] SpamTriggerWords = new[]
    {
        // Urgência artificial
        "URGENTE", "IMEDIATO", "AGORA", "RÁPIDO", "ÚLTIMA CHANCE", "SÓ HOJE",
        "NÃO PERCA", "CORRE", "EXPIRE", "LIMITADO",

        // Ganhos/Dinheiro
        "GRÁTIS", "FREE", "GANHE", "PRÊMIO", "SORTEIO", "LOTERIA",
        "DINHEIRO FÁCIL", "RENDA EXTRA", "LUCRO", "$$$", "💰",

        // Clickbait
        "VOCÊ NÃO VAI ACREDITAR", "CLIQUE AQUI", "ABRA JÁ", "LEIA ISTO",
        "SEGREDO", "REVELADO", "EXCLUSIVO",

        // Saúde/Produtos duvidosos
        "VIAGRA", "CIALIS", "EMAGREÇA", "PERCA PESO", "DIETA MILAGROSA",

        // Financeiro suspeito
        "REFINANCIAMENTO", "EMPRÉSTIMO APROVADO", "CRÉDITO FÁCIL",
        "SEM CONSULTA AO SPC", "DÍVIDA ZERADA",

        // Marketing agressivo
        "COMPRE AGORA", "OFERTA IMPERDÍVEL", "PROMOÇÃO RELÂMPAGO",
        "50% OFF", "70% OFF", "80% OFF", "90% OFF",

        // Excesso de símbolos
        "!!!", "???", "$$$", "%%%"
    };

    // Padrões regex que indicam spam
    private static readonly Regex[] SpamPatterns = new[]
    {
        new Regex(@"[A-Z]{10,}", RegexOptions.Compiled), // Muitas letras maiúsculas seguidas
        new Regex(@"(.)\1{4,}", RegexOptions.Compiled), // Caractere repetido 5+ vezes
        new Regex(@"[!?]{3,}", RegexOptions.Compiled), // Múltiplos ! ou ?
        new Regex(@"[$€£¥]{2,}", RegexOptions.Compiled), // Múltiplos símbolos de moeda
        new Regex(@"(\d+%\s*OFF)", RegexOptions.Compiled | RegexOptions.IgnoreCase) // X% OFF
    };

    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Warnings { get; set; } = new();
        public List<string> Errors { get; set; } = new();
    }

    /// <summary>
    /// Valida o assunto do email contra palavras e padrões de spam
    /// </summary>
    public static ValidationResult Validate(string subject)
    {
        var result = new ValidationResult { IsValid = true };

        if (string.IsNullOrWhiteSpace(subject))
        {
            result.IsValid = false;
            result.Errors.Add("O assunto do email não pode estar vazio");
            return result;
        }

        // Remover tags HTML do subject (caso tenha variáveis com formatação)
        var cleanSubject = Regex.Replace(subject, @"<[^>]+>", "");

        // Verificar tamanho do subject
        if (cleanSubject.Length > 100)
        {
            result.Warnings.Add("Assuntos muito longos podem ser truncados em clientes de email (recomendado: até 50 caracteres)");
        }

        if (cleanSubject.Length < 5)
        {
            result.Warnings.Add("Assuntos muito curtos podem parecer suspeitos");
        }

        // Verificar palavras de spam
        var foundSpamWords = new List<string>();
        foreach (var spamWord in SpamTriggerWords)
        {
            if (cleanSubject.Contains(spamWord, StringComparison.OrdinalIgnoreCase))
            {
                foundSpamWords.Add(spamWord);
            }
        }

        if (foundSpamWords.Any())
        {
            result.Warnings.Add($"Palavras que podem ser consideradas spam detectadas: {string.Join(", ", foundSpamWords)}");
        }

        // Verificar padrões de spam
        foreach (var pattern in SpamPatterns)
        {
            if (pattern.IsMatch(cleanSubject))
            {
                result.Warnings.Add("O assunto contém padrões que podem ser considerados spam (ex: muitas letras maiúsculas, caracteres repetidos, excesso de pontuação)");
                break;
            }
        }

        // Verificar se todo o subject está em MAIÚSCULAS (mais de 50% do texto)
        var upperCaseCount = cleanSubject.Count(char.IsUpper);
        var letterCount = cleanSubject.Count(char.IsLetter);
        if (letterCount > 0 && (double)upperCaseCount / letterCount > 0.7)
        {
            result.Warnings.Add("Evite usar muitas letras MAIÚSCULAS no assunto");
        }

        return result;
    }

    /// <summary>
    /// Retorna sugestões de boas práticas para assuntos de email
    /// </summary>
    public static List<string> GetBestPractices()
    {
        return new List<string>
        {
            "Use de 5 a 50 caracteres no assunto",
            "Evite LETRAS MAIÚSCULAS em excesso",
            "Seja claro e objetivo sobre o conteúdo",
            "Personalize com variáveis (ex: nome do cliente, valor)",
            "Evite palavras como 'grátis', 'urgente', 'promoção'",
            "Evite excesso de pontuação (!!!, ???)",
            "Teste seus assuntos em ferramentas de spam score",
            "Use uma linha de assunto consistente com sua marca"
        };
    }
}
