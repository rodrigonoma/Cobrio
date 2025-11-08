using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cobrio.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CorrigirRegraPadraoEmailObrigatorio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Atualizar réguas padrão "Envio Imediato" para incluir Email nos campos obrigatórios do sistema
            migrationBuilder.Sql(@"
                UPDATE RegraCobranca
                SET VariaveisObrigatoriasSistema = '[""Email""]'
                WHERE Nome = 'Envio Imediato'
                  AND EhPadrao = 1
                  AND (VariaveisObrigatoriasSistema IS NULL
                       OR VariaveisObrigatoriasSistema = ''
                       OR VariaveisObrigatoriasSistema = '[]'
                       OR VariaveisObrigatoriasSistema = 'null');
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
