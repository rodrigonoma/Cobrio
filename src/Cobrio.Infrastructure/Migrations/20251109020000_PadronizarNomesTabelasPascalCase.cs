using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cobrio.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PadronizarNomesTabelasPascalCase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Renomear tabelas que podem estar em lowercase para PascalCase
            // Isso garante consistência entre ambientes Windows (case-insensitive) e Linux (case-sensitive)

            // Verifica e renomeia apenas se necessário (MySQL case-sensitive)
            migrationBuilder.Sql(@"
                SET @table_exists = (SELECT COUNT(*) FROM information_schema.tables
                    WHERE table_schema = DATABASE()
                    AND BINARY table_name = 'empresacliente');

                SET @sql = IF(@table_exists > 0,
                    'RENAME TABLE empresacliente TO EmpresaCliente',
                    'SELECT ''Tabela EmpresaCliente já está OK''');

                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            migrationBuilder.Sql(@"
                SET @table_exists = (SELECT COUNT(*) FROM information_schema.tables
                    WHERE table_schema = DATABASE()
                    AND BINARY table_name = 'usuarioempresa');

                SET @sql = IF(@table_exists > 0,
                    'RENAME TABLE usuarioempresa TO UsuarioEmpresa',
                    'SELECT ''Tabela UsuarioEmpresa já está OK''');

                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            migrationBuilder.Sql(@"
                SET @table_exists = (SELECT COUNT(*) FROM information_schema.tables
                    WHERE table_schema = DATABASE()
                    AND BINARY table_name = 'planooferta');

                SET @sql = IF(@table_exists > 0,
                    'RENAME TABLE planooferta TO PlanoOferta',
                    'SELECT ''Tabela PlanoOferta já está OK''');

                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            migrationBuilder.Sql(@"
                SET @table_exists = (SELECT COUNT(*) FROM information_schema.tables
                    WHERE table_schema = DATABASE()
                    AND BINARY table_name = 'assinante');

                SET @sql = IF(@table_exists > 0,
                    'RENAME TABLE assinante TO Assinante',
                    'SELECT ''Tabela Assinante já está OK''');

                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            migrationBuilder.Sql(@"
                SET @table_exists = (SELECT COUNT(*) FROM information_schema.tables
                    WHERE table_schema = DATABASE()
                    AND BINARY table_name = 'metodopagamento');

                SET @sql = IF(@table_exists > 0,
                    'RENAME TABLE metodopagamento TO MetodoPagamento',
                    'SELECT ''Tabela MetodoPagamento já está OK''');

                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            migrationBuilder.Sql(@"
                SET @table_exists = (SELECT COUNT(*) FROM information_schema.tables
                    WHERE table_schema = DATABASE()
                    AND BINARY table_name = 'fatura');

                SET @sql = IF(@table_exists > 0,
                    'RENAME TABLE fatura TO Fatura',
                    'SELECT ''Tabela Fatura já está OK''');

                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            migrationBuilder.Sql(@"
                SET @table_exists = (SELECT COUNT(*) FROM information_schema.tables
                    WHERE table_schema = DATABASE()
                    AND BINARY table_name = 'itemfatura');

                SET @sql = IF(@table_exists > 0,
                    'RENAME TABLE itemfatura TO ItemFatura',
                    'SELECT ''Tabela ItemFatura já está OK''');

                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            migrationBuilder.Sql(@"
                SET @table_exists = (SELECT COUNT(*) FROM information_schema.tables
                    WHERE table_schema = DATABASE()
                    AND BINARY table_name = 'tentativapagamento');

                SET @sql = IF(@table_exists > 0,
                    'RENAME TABLE tentativapagamento TO TentativaPagamento',
                    'SELECT ''Tabela TentativaPagamento já está OK''');

                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            migrationBuilder.Sql(@"
                SET @table_exists = (SELECT COUNT(*) FROM information_schema.tables
                    WHERE table_schema = DATABASE()
                    AND BINARY table_name = 'reguadunningconfig');

                SET @sql = IF(@table_exists > 0,
                    'RENAME TABLE reguadunningconfig TO ReguaDunningConfig',
                    'SELECT ''Tabela ReguaDunningConfig já está OK''');

                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            migrationBuilder.Sql(@"
                SET @table_exists = (SELECT COUNT(*) FROM information_schema.tables
                    WHERE table_schema = DATABASE()
                    AND BINARY table_name = 'refreshtoken');

                SET @sql = IF(@table_exists > 0,
                    'RENAME TABLE refreshtoken TO RefreshToken',
                    'SELECT ''Tabela RefreshToken já está OK''');

                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            migrationBuilder.Sql(@"
                SET @table_exists = (SELECT COUNT(*) FROM information_schema.tables
                    WHERE table_schema = DATABASE()
                    AND BINARY table_name = 'regracobranca');

                SET @sql = IF(@table_exists > 0,
                    'RENAME TABLE regracobranca TO RegraCobranca',
                    'SELECT ''Tabela RegraCobranca já está OK''');

                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            migrationBuilder.Sql(@"
                SET @table_exists = (SELECT COUNT(*) FROM information_schema.tables
                    WHERE table_schema = DATABASE()
                    AND BINARY table_name = 'cobranca');

                SET @sql = IF(@table_exists > 0,
                    'RENAME TABLE cobranca TO Cobranca',
                    'SELECT ''Tabela Cobranca já está OK''');

                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            migrationBuilder.Sql(@"
                SET @table_exists = (SELECT COUNT(*) FROM information_schema.tables
                    WHERE table_schema = DATABASE()
                    AND BINARY table_name = 'historiconotificacao');

                SET @sql = IF(@table_exists > 0,
                    'RENAME TABLE historiconotificacao TO HistoricoNotificacao',
                    'SELECT ''Tabela HistoricoNotificacao já está OK''');

                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            migrationBuilder.Sql(@"
                SET @table_exists = (SELECT COUNT(*) FROM information_schema.tables
                    WHERE table_schema = DATABASE()
                    AND BINARY table_name = 'historicoimportacao');

                SET @sql = IF(@table_exists > 0,
                    'RENAME TABLE historicoimportacao TO HistoricoImportacao',
                    'SELECT ''Tabela HistoricoImportacao já está OK''');

                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            migrationBuilder.Sql(@"
                SET @table_exists = (SELECT COUNT(*) FROM information_schema.tables
                    WHERE table_schema = DATABASE()
                    AND BINARY table_name = 'modulo');

                SET @sql = IF(@table_exists > 0,
                    'RENAME TABLE modulo TO Modulo',
                    'SELECT ''Tabela Modulo já está OK''');

                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            migrationBuilder.Sql(@"
                SET @table_exists = (SELECT COUNT(*) FROM information_schema.tables
                    WHERE table_schema = DATABASE()
                    AND BINARY table_name = 'acao');

                SET @sql = IF(@table_exists > 0,
                    'RENAME TABLE acao TO Acao',
                    'SELECT ''Tabela Acao já está OK''');

                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            migrationBuilder.Sql(@"
                SET @table_exists = (SELECT COUNT(*) FROM information_schema.tables
                    WHERE table_schema = DATABASE()
                    AND BINARY table_name = 'permissaoperfil');

                SET @sql = IF(@table_exists > 0,
                    'RENAME TABLE permissaoperfil TO PermissaoPerfil',
                    'SELECT ''Tabela PermissaoPerfil já está OK''');

                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            migrationBuilder.Sql(@"
                SET @table_exists = (SELECT COUNT(*) FROM information_schema.tables
                    WHERE table_schema = DATABASE()
                    AND BINARY table_name = 'configuracaoemail');

                SET @sql = IF(@table_exists > 0,
                    'RENAME TABLE configuracaoemail TO ConfiguracaoEmail',
                    'SELECT ''Tabela ConfiguracaoEmail já está OK''');

                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reverter as mudanças (renomear de volta para lowercase)
            // Nota: Isso só será necessário se você precisar fazer rollback

            migrationBuilder.RenameTable(name: "EmpresaCliente", newName: "empresacliente");
            migrationBuilder.RenameTable(name: "UsuarioEmpresa", newName: "usuarioempresa");
            migrationBuilder.RenameTable(name: "PlanoOferta", newName: "planooferta");
            migrationBuilder.RenameTable(name: "Assinante", newName: "assinante");
            migrationBuilder.RenameTable(name: "MetodoPagamento", newName: "metodopagamento");
            migrationBuilder.RenameTable(name: "Fatura", newName: "fatura");
            migrationBuilder.RenameTable(name: "ItemFatura", newName: "itemfatura");
            migrationBuilder.RenameTable(name: "TentativaPagamento", newName: "tentativapagamento");
            migrationBuilder.RenameTable(name: "ReguaDunningConfig", newName: "reguadunningconfig");
            migrationBuilder.RenameTable(name: "RefreshToken", newName: "refreshtoken");
            migrationBuilder.RenameTable(name: "RegraCobranca", newName: "regracobranca");
            migrationBuilder.RenameTable(name: "Cobranca", newName: "cobranca");
            migrationBuilder.RenameTable(name: "HistoricoNotificacao", newName: "historiconotificacao");
            migrationBuilder.RenameTable(name: "HistoricoImportacao", newName: "historicoimportacao");
            migrationBuilder.RenameTable(name: "Modulo", newName: "modulo");
            migrationBuilder.RenameTable(name: "Acao", newName: "acao");
            migrationBuilder.RenameTable(name: "PermissaoPerfil", newName: "permissaoperfil");
            migrationBuilder.RenameTable(name: "ConfiguracaoEmail", newName: "configuracaoemail");
        }
    }
}
