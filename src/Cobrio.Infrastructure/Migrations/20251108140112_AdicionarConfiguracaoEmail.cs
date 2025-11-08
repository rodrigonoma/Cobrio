using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cobrio.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarConfiguracaoEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SubjectEmail",
                table: "RegraCobranca",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "EmailRemetente",
                table: "EmpresaCliente",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "EmailReplyTo",
                table: "EmpresaCliente",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "NomeRemetente",
                table: "EmpresaCliente",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SubjectEmail",
                table: "RegraCobranca");

            migrationBuilder.DropColumn(
                name: "EmailRemetente",
                table: "EmpresaCliente");

            migrationBuilder.DropColumn(
                name: "EmailReplyTo",
                table: "EmpresaCliente");

            migrationBuilder.DropColumn(
                name: "NomeRemetente",
                table: "EmpresaCliente");
        }
    }
}
