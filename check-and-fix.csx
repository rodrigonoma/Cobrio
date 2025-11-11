#!/usr/bin/env dotnet-script
#r "nuget: MySql.Data, 8.0.33"

using MySql.Data.MySqlClient;

Console.WriteLine("=== VERIFICANDO BANCO LOCAL ===\n");
var localConn = "Server=localhost;Port=3306;Database=cobranca_db;Uid=root;Pwd=Root@123;";
CheckAndFix("LOCAL", localConn);

Console.WriteLine("\n=== VERIFICANDO BANCO PRODUÇÃO ===\n");
var prodConn = "Server=aws.connect.psdb.cloud;Database=cobranca_db;Uid=zzkvxr5c6x3d0h7d65ni;Pwd=pscale_pw_MN8VSwledKkeyZjfQOJWEdjGT2fCnAMIg46r8gv88FI;SslMode=Required;";
CheckAndFix("PRODUÇÃO", prodConn);

void CheckAndFix(string env, string connectionString)
{
    try
    {
        using var conn = new MySqlConnection(connectionString);
        conn.Open();
        Console.WriteLine($"✓ Conectado ao banco {env}");

        // Verificar estado atual
        var selectCmd = new MySqlCommand(@"
            SELECT Id, Nome, EhPadrao, TipoMomento, ValorTempo, UnidadeTempo,
                   CanalNotificacao, SubjectEmail, VariaveisObrigatorias
            FROM RegraCobranca
            WHERE Nome = 'Envio Imediato' OR EhPadrao = 1
            LIMIT 1
        ", conn);

        using var reader = selectCmd.ExecuteReader();
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

        Console.WriteLine($"\nREGRA ATUAL em {env}:");
        Console.WriteLine($"  Nome: {nome}");
        Console.WriteLine($"  TipoMomento: {tipoMomento}");
        Console.WriteLine($"  ValorTempo: {valorTempo}");
        Console.WriteLine($"  UnidadeTempo: {unidadeTempo} ({GetUnidadeNome(unidadeTempo)})");
        Console.WriteLine($"  CanalNotificacao: {canalNotificacao}");
        Console.WriteLine($"  SubjectEmail: {subjectEmail ?? "(vazio)"}");
        Console.WriteLine($"  VariaveisObrigatorias: {variaveisObrigatorias}");

        reader.Close();

        // Verificar se precisa corrigir
        bool needsFix = unidadeTempo != 1 || valorTempo != 1 || tipoMomento != 0;

        if (needsFix)
        {
            Console.WriteLine($"\n⚠ CORREÇÃO NECESSÁRIA em {env}!");
            Console.WriteLine("  Atualizando para: TipoMomento=0 (Antes), ValorTempo=1, UnidadeTempo=1 (Minutos)");

            var updateCmd = new MySqlCommand(@"
                UPDATE RegraCobranca
                SET TipoMomento = 0, ValorTempo = 1, UnidadeTempo = 1, AtualizadoEm = UTC_TIMESTAMP()
                WHERE Id = @id
            ", conn);
            updateCmd.Parameters.AddWithValue("@id", id.ToString());

            var rows = updateCmd.ExecuteNonQuery();
            Console.WriteLine($"✓ Atualizado {rows} registro(s) em {env}");
        }
        else
        {
            Console.WriteLine($"\n✓ Regra está correta em {env}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"✗ ERRO em {env}: {ex.Message}");
    }
}

string GetUnidadeNome(int unidade)
{
    return unidade switch
    {
        1 => "Minutos",
        2 => "Horas",
        3 => "Dias",
        _ => "Desconhecido"
    };
}
