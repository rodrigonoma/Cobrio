using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cobrio.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarRastreamentoEventosBrevo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CodigoErroProvedor",
                table: "HistoricoNotificacao",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "DataPrimeiraAbertura",
                table: "HistoricoNotificacao",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataPrimeiroClique",
                table: "HistoricoNotificacao",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataUltimaAbertura",
                table: "HistoricoNotificacao",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataUltimoClique",
                table: "HistoricoNotificacao",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IpAbertura",
                table: "HistoricoNotificacao",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "LinkClicado",
                table: "HistoricoNotificacao",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "MessageIdProvedor",
                table: "HistoricoNotificacao",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "MotivoRejeicao",
                table: "HistoricoNotificacao",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "QuantidadeAberturas",
                table: "HistoricoNotificacao",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "QuantidadeCliques",
                table: "HistoricoNotificacao",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "UserAgentAbertura",
                table: "HistoricoNotificacao",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CodigoErroProvedor",
                table: "HistoricoNotificacao");

            migrationBuilder.DropColumn(
                name: "DataPrimeiraAbertura",
                table: "HistoricoNotificacao");

            migrationBuilder.DropColumn(
                name: "DataPrimeiroClique",
                table: "HistoricoNotificacao");

            migrationBuilder.DropColumn(
                name: "DataUltimaAbertura",
                table: "HistoricoNotificacao");

            migrationBuilder.DropColumn(
                name: "DataUltimoClique",
                table: "HistoricoNotificacao");

            migrationBuilder.DropColumn(
                name: "IpAbertura",
                table: "HistoricoNotificacao");

            migrationBuilder.DropColumn(
                name: "LinkClicado",
                table: "HistoricoNotificacao");

            migrationBuilder.DropColumn(
                name: "MessageIdProvedor",
                table: "HistoricoNotificacao");

            migrationBuilder.DropColumn(
                name: "MotivoRejeicao",
                table: "HistoricoNotificacao");

            migrationBuilder.DropColumn(
                name: "QuantidadeAberturas",
                table: "HistoricoNotificacao");

            migrationBuilder.DropColumn(
                name: "QuantidadeCliques",
                table: "HistoricoNotificacao");

            migrationBuilder.DropColumn(
                name: "UserAgentAbertura",
                table: "HistoricoNotificacao");
        }
    }
}
