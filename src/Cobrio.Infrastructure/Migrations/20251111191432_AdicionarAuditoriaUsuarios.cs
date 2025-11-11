using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cobrio.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarAuditoriaUsuarios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UsuarioCriacaoId",
                table: "UsuarioEmpresa",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "UsuarioModificacaoId",
                table: "UsuarioEmpresa",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "UsuarioCriacaoId",
                table: "TentativaPagamento",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "UsuarioModificacaoId",
                table: "TentativaPagamento",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "UsuarioCriacaoId",
                table: "TemplateEmail",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "UsuarioModificacaoId",
                table: "TemplateEmail",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "UsuarioCriacaoId",
                table: "ReguaDunningConfig",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "UsuarioModificacaoId",
                table: "ReguaDunningConfig",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "UsuarioCriacaoId",
                table: "RegraCobranca",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "UsuarioModificacaoId",
                table: "RegraCobranca",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "UsuarioCriacaoId",
                table: "RefreshToken",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "UsuarioModificacaoId",
                table: "RefreshToken",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "UsuarioCriacaoId",
                table: "PlanoOferta",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "UsuarioModificacaoId",
                table: "PlanoOferta",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "UsuarioCriacaoId",
                table: "PermissaoPerfil",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "UsuarioModificacaoId",
                table: "PermissaoPerfil",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "UsuarioCriacaoId",
                table: "Modulo",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "UsuarioModificacaoId",
                table: "Modulo",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "UsuarioCriacaoId",
                table: "MetodoPagamento",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "UsuarioModificacaoId",
                table: "MetodoPagamento",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "UsuarioCriacaoId",
                table: "ItemFatura",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "UsuarioModificacaoId",
                table: "ItemFatura",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "UsuarioCriacaoId",
                table: "HistoricoStatusNotificacao",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "UsuarioModificacaoId",
                table: "HistoricoStatusNotificacao",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "UsuarioCriacaoId",
                table: "HistoricoNotificacao",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "UsuarioModificacaoId",
                table: "HistoricoNotificacao",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "UsuarioCriacaoId",
                table: "HistoricoImportacao",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "UsuarioModificacaoId",
                table: "HistoricoImportacao",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "UsuarioCriacaoId",
                table: "Fatura",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "UsuarioModificacaoId",
                table: "Fatura",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "UsuarioCriacaoId",
                table: "EmpresaCliente",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "UsuarioModificacaoId",
                table: "EmpresaCliente",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "UsuarioCriacaoId",
                table: "Cobranca",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "UsuarioModificacaoId",
                table: "Cobranca",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "UsuarioCriacaoId",
                table: "Assinante",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "UsuarioModificacaoId",
                table: "Assinante",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "UsuarioCriacaoId",
                table: "Acao",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "UsuarioModificacaoId",
                table: "Acao",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UsuarioCriacaoId",
                table: "UsuarioEmpresa");

            migrationBuilder.DropColumn(
                name: "UsuarioModificacaoId",
                table: "UsuarioEmpresa");

            migrationBuilder.DropColumn(
                name: "UsuarioCriacaoId",
                table: "TentativaPagamento");

            migrationBuilder.DropColumn(
                name: "UsuarioModificacaoId",
                table: "TentativaPagamento");

            migrationBuilder.DropColumn(
                name: "UsuarioCriacaoId",
                table: "TemplateEmail");

            migrationBuilder.DropColumn(
                name: "UsuarioModificacaoId",
                table: "TemplateEmail");

            migrationBuilder.DropColumn(
                name: "UsuarioCriacaoId",
                table: "ReguaDunningConfig");

            migrationBuilder.DropColumn(
                name: "UsuarioModificacaoId",
                table: "ReguaDunningConfig");

            migrationBuilder.DropColumn(
                name: "UsuarioCriacaoId",
                table: "RegraCobranca");

            migrationBuilder.DropColumn(
                name: "UsuarioModificacaoId",
                table: "RegraCobranca");

            migrationBuilder.DropColumn(
                name: "UsuarioCriacaoId",
                table: "RefreshToken");

            migrationBuilder.DropColumn(
                name: "UsuarioModificacaoId",
                table: "RefreshToken");

            migrationBuilder.DropColumn(
                name: "UsuarioCriacaoId",
                table: "PlanoOferta");

            migrationBuilder.DropColumn(
                name: "UsuarioModificacaoId",
                table: "PlanoOferta");

            migrationBuilder.DropColumn(
                name: "UsuarioCriacaoId",
                table: "PermissaoPerfil");

            migrationBuilder.DropColumn(
                name: "UsuarioModificacaoId",
                table: "PermissaoPerfil");

            migrationBuilder.DropColumn(
                name: "UsuarioCriacaoId",
                table: "Modulo");

            migrationBuilder.DropColumn(
                name: "UsuarioModificacaoId",
                table: "Modulo");

            migrationBuilder.DropColumn(
                name: "UsuarioCriacaoId",
                table: "MetodoPagamento");

            migrationBuilder.DropColumn(
                name: "UsuarioModificacaoId",
                table: "MetodoPagamento");

            migrationBuilder.DropColumn(
                name: "UsuarioCriacaoId",
                table: "ItemFatura");

            migrationBuilder.DropColumn(
                name: "UsuarioModificacaoId",
                table: "ItemFatura");

            migrationBuilder.DropColumn(
                name: "UsuarioCriacaoId",
                table: "HistoricoStatusNotificacao");

            migrationBuilder.DropColumn(
                name: "UsuarioModificacaoId",
                table: "HistoricoStatusNotificacao");

            migrationBuilder.DropColumn(
                name: "UsuarioCriacaoId",
                table: "HistoricoNotificacao");

            migrationBuilder.DropColumn(
                name: "UsuarioModificacaoId",
                table: "HistoricoNotificacao");

            migrationBuilder.DropColumn(
                name: "UsuarioCriacaoId",
                table: "HistoricoImportacao");

            migrationBuilder.DropColumn(
                name: "UsuarioModificacaoId",
                table: "HistoricoImportacao");

            migrationBuilder.DropColumn(
                name: "UsuarioCriacaoId",
                table: "Fatura");

            migrationBuilder.DropColumn(
                name: "UsuarioModificacaoId",
                table: "Fatura");

            migrationBuilder.DropColumn(
                name: "UsuarioCriacaoId",
                table: "EmpresaCliente");

            migrationBuilder.DropColumn(
                name: "UsuarioModificacaoId",
                table: "EmpresaCliente");

            migrationBuilder.DropColumn(
                name: "UsuarioCriacaoId",
                table: "Cobranca");

            migrationBuilder.DropColumn(
                name: "UsuarioModificacaoId",
                table: "Cobranca");

            migrationBuilder.DropColumn(
                name: "UsuarioCriacaoId",
                table: "Assinante");

            migrationBuilder.DropColumn(
                name: "UsuarioModificacaoId",
                table: "Assinante");

            migrationBuilder.DropColumn(
                name: "UsuarioCriacaoId",
                table: "Acao");

            migrationBuilder.DropColumn(
                name: "UsuarioModificacaoId",
                table: "Acao");
        }
    }
}
