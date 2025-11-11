using System;
using MySql.Data.MySqlClient;

var localConn = "Server=127.0.0.1;Database=Cobrio;User=root;Password=root;CharSet=utf8mb4;";
var prodConn = "Server=72.60.63.64;Database=cobrio;User=cobrio_user;Password=A$HAi8hA82%;CharSet=utf8mb4;SslMode=Required;";

Console.WriteLine("=== VERIFICANDO BANCO LOCAL ===\n");
CheckAndFix("LOCAL", localConn);

Console.WriteLine("\n" + new string('=', 80) + "\n");

Console.WriteLine("=== VERIFICANDO BANCO PRODUÇÃO ===\n");
CheckAndFix("PRODUCAO", prodConn);

void CheckAndFix(string env, string connStr)
{
    try
    {
        using var conn = new MySqlConnection(connStr);
        conn.Open();
        Console.WriteLine($"✓ Conectado ao banco {env}\n");

        // Buscar a regra
        var cmd = new MySqlCommand(@"
            SELECT Id, Nome, EhPadrao, TipoMomento, ValorTempo, UnidadeTempo,
                   CanalNotificacao, SubjectEmail, VariaveisObrigatorias
            FROM RegraCobranca
            WHERE Nome = 'Envio Imediato' OR EhPadrao = 1
            LIMIT 1
        ", conn);

        using var reader = cmd.ExecuteReader();
        if (!reader.Read())
        {
            Console.WriteLine($"⚠ Nenhuma regra 'Envio Imediato' encontrada em {env}");
            return;
        }

        var id = reader.GetGuid(0);
        var nome = reader.GetString(1);
        var ehPadrao = reader.GetBoolean(2);
        var tipoMomento = reader.GetInt32(3);
        var valorTempo = reader.GetInt32(4);
        var unidadeTempo = reader.GetInt32(5);
        var canalNotificacao = reader.GetInt32(6);
        var subjectEmail = reader.IsDBNull(7) ? null : reader.GetString(7);
        var variaveisObrigatorias = reader.GetString(8);

        Console.WriteLine($"ESTADO ATUAL:");
        Console.WriteLine($"  Id: {id}");
        Console.WriteLine($"  Nome: {nome}");
        Console.WriteLine($"  EhPadrao: {ehPadrao}");
        Console.WriteLine($"  TipoMomento: {tipoMomento} ({GetTipoMomentoNome(tipoMomento)})");
        Console.WriteLine($"  ValorTempo: {valorTempo}");
        Console.WriteLine($"  UnidadeTempo: {unidadeTempo} ({GetUnidadeTempoNome(unidadeTempo)})");
        Console.WriteLine($"  CanalNotificacao: {canalNotificacao}");
        Console.WriteLine($"  SubjectEmail: {subjectEmail ?? "(NULL)"}");
        Console.WriteLine($"  VariaveisObrigatorias: {variaveisObrigatorias}");

        reader.Close();

        // Verificar se precisa corrigir
        bool needsFix = false;
        string problemas = "";

        if (tipoMomento != 0)
        {
            needsFix = true;
            problemas += $"  - TipoMomento está {tipoMomento}, deveria ser 0 (Antes)\n";
        }

        if (valorTempo != 1)
        {
            needsFix = true;
            problemas += $"  - ValorTempo está {valorTempo}, deveria ser 1\n";
        }

        if (unidadeTempo != 1)
        {
            needsFix = true;
            problemas += $"  - UnidadeTempo está {unidadeTempo}, deveria ser 1 (Minutos)\n";
        }

        if (needsFix)
        {
            Console.WriteLine($"\n⚠ PROBLEMAS ENCONTRADOS em {env}:");
            Console.WriteLine(problemas);
            Console.WriteLine("Corrigindo...");

            var updateCmd = new MySqlCommand(@"
                UPDATE RegraCobranca
                SET TipoMomento = 0, ValorTempo = 1, UnidadeTempo = 1
                WHERE Id = @id
            ", conn);
            updateCmd.Parameters.AddWithValue("@id", id.ToString());

            var rowsAffected = updateCmd.ExecuteNonQuery();
            Console.WriteLine($"✓ {rowsAffected} registro(s) atualizado(s) com sucesso em {env}");
        }
        else
        {
            Console.WriteLine($"\n✓ Regra está CORRETA em {env} - Nenhuma correção necessária");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"✗ ERRO ao conectar em {env}:");
        Console.WriteLine($"  Mensagem: {ex.Message}");
        if (ex.InnerException != null)
        {
            Console.WriteLine($"  Inner: {ex.InnerException.Message}");
        }
    }
}

string GetTipoMomentoNome(int tipo) => tipo switch
{
    0 => "Antes",
    1 => "Depois",
    2 => "No Vencimento",
    _ => "Desconhecido"
};

string GetUnidadeTempoNome(int unidade) => unidade switch
{
    1 => "Minutos",
    2 => "Horas",
    3 => "Dias",
    _ => "Desconhecido"
};
