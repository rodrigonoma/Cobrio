using System;
using System.Data;
using MySql.Data.MySqlClient;

var localConn = "Server=127.0.0.1;Database=Cobrio;User=root;Password=root;CharSet=utf8mb4;SslMode=None;";
var prodConn = "Server=72.60.63.64;Database=cobrio;User=cobrio_user;Password=A$HAi8hA82%;CharSet=utf8mb4;SslMode=Required;";

Console.WriteLine("=== BANCO LOCAL ===");
Check(localConn);

Console.WriteLine("\n=== BANCO PRODUÇÃO ===");
Check(prodConn);

void Check(string connStr)
{
    try
    {
        using var conn = new MySqlConnection(connStr);
        conn.Open();

        var cmd = new MySqlCommand(@"
            SELECT Id, Nome, EhPadrao, TipoMomento, ValorTempo, UnidadeTempo,
                   CanalNotificacao, SubjectEmail, VariaveisObrigatorias
            FROM RegraCobranca
            WHERE Nome = 'Envio Imediato' OR EhPadrao = 1
            LIMIT 1
        ", conn);

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            Console.WriteLine($"Id: {reader.GetString(0)}");
            Console.WriteLine($"Nome: {reader.GetString(1)}");
            Console.WriteLine($"EhPadrao: {reader.GetBoolean(2)}");
            Console.WriteLine($"TipoMomento: {reader.GetInt32(3)}");
            Console.WriteLine($"ValorTempo: {reader.GetInt32(4)}");
            Console.WriteLine($"UnidadeTempo: {reader.GetInt32(5)}");
            Console.WriteLine($"CanalNotificacao: {reader.GetInt32(6)}");
            Console.WriteLine($"SubjectEmail: {(reader.IsDBNull(7) ? "NULL" : reader.GetString(7))}");
            Console.WriteLine($"VariaveisObrigatorias: {reader.GetString(8)}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"ERRO: {ex.Message}");
    }
}
