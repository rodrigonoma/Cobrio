#!/usr/bin/env dotnet-script
#r "nuget: MySql.Data, 8.0.33"
#r "nuget: System.Text.Json, 8.0.0"

using MySql.Data.MySqlClient;
using System.Text.Json;
using System.Text.RegularExpressions;

// Configuração do banco de dados
var connectionString = "Server=localhost;Port=3306;Database=cobranca_db;Uid=root;Pwd=Root@123;";

Console.WriteLine("Atualizando variáveis das regras com SubjectEmail...\n");

using var connection = new MySqlConnection(connectionString);
connection.Open();

// Buscar todas as regras que têm SubjectEmail
var selectCmd = new MySqlCommand(@"
    SELECT Id, Nome, TemplateNotificacao, SubjectEmail, VariaveisObrigatorias
    FROM RegraCobranca
    WHERE SubjectEmail IS NOT NULL AND SubjectEmail != ''
", connection);

var regras = new List<(Guid Id, string Nome, string Template, string Subject, string VariaveisAtuais)>();

using (var reader = selectCmd.ExecuteReader())
{
    while (reader.Read())
    {
        regras.Add((
            Guid.Parse(reader.GetString(0)),
            reader.GetString(1),
            reader.GetString(2),
            reader.GetString(3),
            reader.GetString(4)
        ));
    }
}

Console.WriteLine($"Encontradas {regras.Count} regras com SubjectEmail.\n");

foreach (var regra in regras)
{
    Console.WriteLine($"Processando: {regra.Nome}");
    Console.WriteLine($"  Template: {regra.Template.Substring(0, Math.Min(50, regra.Template.Length))}...");
    Console.WriteLine($"  Subject: {regra.Subject}");

    // Extrair variáveis do template
    var variaveisTemplate = ExtrairVariaveis(regra.Template);

    // Extrair variáveis do subject
    var variaveisSubject = ExtrairVariaveis(regra.Subject);

    // Combinar e remover duplicatas
    var todasVariaveis = variaveisTemplate.Union(variaveisSubject).ToList();

    // Serializar para JSON
    var novasVariaveisJson = JsonSerializer.Serialize(todasVariaveis);

    Console.WriteLine($"  Variáveis antigas: {regra.VariaveisAtuais}");
    Console.WriteLine($"  Variáveis novas: {novasVariaveisJson}");

    // Atualizar no banco
    var updateCmd = new MySqlCommand(@"
        UPDATE RegraCobranca
        SET VariaveisObrigatorias = @variaveis,
            AtualizadoEm = @agora
        WHERE Id = @id
    ", connection);

    updateCmd.Parameters.AddWithValue("@variaveis", novasVariaveisJson);
    updateCmd.Parameters.AddWithValue("@agora", DateTime.UtcNow);
    updateCmd.Parameters.AddWithValue("@id", regra.Id.ToString());

    var rowsAffected = updateCmd.ExecuteNonQuery();
    Console.WriteLine($"  Atualizado: {rowsAffected} linha(s)\n");
}

Console.WriteLine($"Concluído! {regras.Count} regras atualizadas.");

List<string> ExtrairVariaveis(string template)
{
    var regex = new Regex(@"\{\{([^}]+)\}\}");
    var matches = regex.Matches(template);
    var variaveis = new HashSet<string>();

    foreach (Match match in matches)
    {
        if (match.Groups.Count > 1)
        {
            var variavel = match.Groups[1].Value.Trim();

            // Limpar HTML
            variavel = Regex.Replace(variavel, @"<\/?[^>]+(>|$)", "");
            variavel = Regex.Replace(variavel, @"&nbsp;", " ");
            variavel = Regex.Replace(variavel, @"&[a-z]+;", "", RegexOptions.IgnoreCase);
            variavel = Regex.Replace(variavel, @"\s+", " ").Trim();

            if (!string.IsNullOrEmpty(variavel))
            {
                variaveis.Add(variavel);
            }
        }
    }

    return variaveis.ToList();
}
