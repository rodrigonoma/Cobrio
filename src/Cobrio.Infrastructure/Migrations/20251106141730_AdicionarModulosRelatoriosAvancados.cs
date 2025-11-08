using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cobrio.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarModulosRelatoriosAvancados : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Adicionar ação "Visualizar"
            migrationBuilder.Sql(@"
                INSERT INTO Acao (Id, Nome, Chave, Descricao, TipoAcao, Ativa, CriadoEm, AtualizadoEm)
                VALUES (UUID(), 'Visualizar', 'visualizar', 'Visualizar conteúdo', 'CRUD', 1, NOW(), NOW());
            ");

            // Adicionar módulos de relatórios avançados
            migrationBuilder.Sql(@"
                INSERT INTO Modulo (Id, Nome, Chave, Descricao, Icone, Rota, Ordem, Ativo, CriadoEm, AtualizadoEm)
                VALUES
                (UUID(), 'Relatórios Operacionais', 'relatorios-operacionais', 'Relatórios operacionais e execução de cobranças', 'pi-chart-line', '/relatorios', 8, 1, NOW(), NOW()),
                (UUID(), 'Relatórios Gerenciais', 'relatorios-gerenciais', 'Relatórios gerenciais e análises estratégicas', 'pi-chart-pie', '/relatorios', 9, 1, NOW(), NOW());
            ");

            // Atualizar ordem do módulo de Permissões para 10
            migrationBuilder.Sql(@"
                UPDATE Modulo SET Ordem = 10 WHERE Chave = 'permissoes';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remover módulos de relatórios avançados
            migrationBuilder.Sql(@"
                DELETE FROM Modulo WHERE Chave IN ('relatorios-operacionais', 'relatorios-gerenciais');
            ");

            // Remover ação "Visualizar"
            migrationBuilder.Sql(@"
                DELETE FROM Acao WHERE Chave = 'visualizar';
            ");

            // Restaurar ordem original do módulo de Permissões
            migrationBuilder.Sql(@"
                UPDATE Modulo SET Ordem = 8 WHERE Chave = 'permissoes';
            ");
        }
    }
}
