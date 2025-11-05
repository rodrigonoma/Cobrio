using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cobrio.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateWithRegraCobranca : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Acao",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    Nome = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Chave = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Descricao = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TipoAcao = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Ativa = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    CriadoEm = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Acao", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "EmpresaCliente",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    Nome = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CNPJ = table.Column<string>(type: "CHAR(14)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Telefone = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PlanoCobrioId = table.Column<int>(type: "int", nullable: false),
                    DataContrato = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    StatusContrato = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Endereco_Logradouro = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Endereco_Numero = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Endereco_Complemento = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Endereco_Bairro = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Endereco_Cidade = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Endereco_Estado = table.Column<string>(type: "CHAR(2)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Endereco_CEP = table.Column<string>(type: "CHAR(8)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Endereco_Pais = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true, defaultValue: "Brasil")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CriadoEm = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmpresaCliente", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Modulo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    Nome = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Chave = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Descricao = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Icone = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false, defaultValue: "pi-circle")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Rota = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Ordem = table.Column<int>(type: "int", nullable: false),
                    Ativo = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    CriadoEm = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Modulo", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PlanoOferta",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    EmpresaClienteId = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    Nome = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Descricao = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TipoCiclo = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ValorCentavos = table.Column<long>(type: "bigint", nullable: false),
                    PeriodoTrial = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    LimiteUsuarios = table.Column<int>(type: "int", nullable: true),
                    PermiteDowngrade = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    PermiteUpgrade = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    Ativo = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    Moeda = table.Column<string>(type: "CHAR(3)", nullable: true, defaultValue: "BRL")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CriadoEm = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanoOferta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlanoOferta_EmpresaCliente_EmpresaClienteId",
                        column: x => x.EmpresaClienteId,
                        principalTable: "EmpresaCliente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RegraCobranca",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    EmpresaClienteId = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    Nome = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Descricao = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Ativa = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    EhPadrao = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    TipoMomento = table.Column<int>(type: "int", nullable: false),
                    ValorTempo = table.Column<int>(type: "int", nullable: false),
                    UnidadeTempo = table.Column<int>(type: "int", nullable: false),
                    CanalNotificacao = table.Column<int>(type: "int", nullable: false),
                    TemplateNotificacao = table.Column<string>(type: "TEXT", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    VariaveisObrigatorias = table.Column<string>(type: "TEXT", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    VariaveisObrigatoriasSistema = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TokenWebhook = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CriadoEm = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegraCobranca", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RegraCobranca_EmpresaCliente_EmpresaClienteId",
                        column: x => x.EmpresaClienteId,
                        principalTable: "EmpresaCliente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ReguaDunningConfig",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    EmpresaClienteId = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    NumeroMaximoTentativas = table.Column<int>(type: "int", nullable: false, defaultValue: 3),
                    IntervalosDiasJson = table.Column<string>(type: "JSON", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EnviarEmail = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    EnviarSMS = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    EnviarNotificacaoInApp = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    DiasSuspensao = table.Column<int>(type: "int", nullable: false, defaultValue: 15),
                    DiasCancelamento = table.Column<int>(type: "int", nullable: false, defaultValue: 30),
                    HoraInicioRetry = table.Column<TimeSpan>(type: "time(6)", nullable: false, defaultValue: new TimeSpan(0, 8, 0, 0, 0)),
                    HoraFimRetry = table.Column<TimeSpan>(type: "time(6)", nullable: false, defaultValue: new TimeSpan(0, 20, 0, 0, 0)),
                    Ativo = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    CriadoEm = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReguaDunningConfig", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReguaDunningConfig_EmpresaCliente_EmpresaClienteId",
                        column: x => x.EmpresaClienteId,
                        principalTable: "EmpresaCliente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UsuarioEmpresa",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    EmpresaClienteId = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    Nome = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PasswordHash = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Perfil = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValueSql: "'Operador'")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Ativo = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    EhProprietario = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    UltimoAcesso = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuarioEmpresa", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UsuarioEmpresa_EmpresaCliente_EmpresaClienteId",
                        column: x => x.EmpresaClienteId,
                        principalTable: "EmpresaCliente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Assinante",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    EmpresaClienteId = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    PlanoOfertaId = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    Nome = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CPFCNPJ = table.Column<string>(type: "varchar(14)", maxLength: 14, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Telefone = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataInicio = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DataFimCiclo = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DataCancelamento = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    MotivoCancelamento = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EmTrial = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    DataFimTrial = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DiaVencimento = table.Column<int>(type: "int", nullable: false),
                    ProximaCobranca = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Endereco_Logradouro = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Endereco_Numero = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Endereco_Complemento = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Endereco_Bairro = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Endereco_Cidade = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Endereco_Estado = table.Column<string>(type: "CHAR(2)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Endereco_CEP = table.Column<string>(type: "CHAR(8)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Endereco_Pais = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true, defaultValue: "Brasil")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CriadoEm = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assinante", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Assinante_EmpresaCliente_EmpresaClienteId",
                        column: x => x.EmpresaClienteId,
                        principalTable: "EmpresaCliente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Assinante_PlanoOferta_PlanoOfertaId",
                        column: x => x.PlanoOfertaId,
                        principalTable: "PlanoOferta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Cobranca",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    RegraCobrancaId = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    EmpresaClienteId = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    PayloadJson = table.Column<string>(type: "TEXT", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataVencimento = table.Column<DateTime>(type: "DATE", nullable: false),
                    DataDisparo = table.Column<DateTime>(type: "DATE", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TentativasEnvio = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    DataProcessamento = table.Column<DateTime>(type: "DATETIME", nullable: true),
                    MensagemErro = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CriadoEm = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cobranca", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cobranca_EmpresaCliente_EmpresaClienteId",
                        column: x => x.EmpresaClienteId,
                        principalTable: "EmpresaCliente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Cobranca_RegraCobranca_RegraCobrancaId",
                        column: x => x.RegraCobrancaId,
                        principalTable: "RegraCobranca",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "HistoricoImportacao",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    RegraCobrancaId = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    EmpresaClienteId = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    UsuarioId = table.Column<Guid>(type: "CHAR(36)", nullable: true, collation: "ascii_general_ci"),
                    NomeArquivo = table.Column<string>(type: "VARCHAR(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataImportacao = table.Column<DateTime>(type: "DATETIME", nullable: false),
                    Origem = table.Column<int>(type: "int", nullable: false),
                    TotalLinhas = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    LinhasProcessadas = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    LinhasComErro = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ErrosJson = table.Column<string>(type: "LONGTEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CriadoEm = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoricoImportacao", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistoricoImportacao_EmpresaCliente_EmpresaClienteId",
                        column: x => x.EmpresaClienteId,
                        principalTable: "EmpresaCliente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HistoricoImportacao_RegraCobranca_RegraCobrancaId",
                        column: x => x.RegraCobrancaId,
                        principalTable: "RegraCobranca",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HistoricoImportacao_UsuarioEmpresa_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "UsuarioEmpresa",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PermissaoPerfil",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    EmpresaClienteId = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    PerfilUsuario = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ModuloId = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    AcaoId = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    Permitido = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    CriadoPorUsuarioId = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    CriadoEm = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissaoPerfil", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PermissaoPerfil_Acao_AcaoId",
                        column: x => x.AcaoId,
                        principalTable: "Acao",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PermissaoPerfil_EmpresaCliente_EmpresaClienteId",
                        column: x => x.EmpresaClienteId,
                        principalTable: "EmpresaCliente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PermissaoPerfil_Modulo_ModuloId",
                        column: x => x.ModuloId,
                        principalTable: "Modulo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PermissaoPerfil_UsuarioEmpresa_CriadoPorUsuarioId",
                        column: x => x.CriadoPorUsuarioId,
                        principalTable: "UsuarioEmpresa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RefreshToken",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    UsuarioEmpresaId = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    Token = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ExpiresAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    IsRevoked = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    RevokedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    RevokedByIp = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedByIp = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CriadoEm = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshToken", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshToken_UsuarioEmpresa_UsuarioEmpresaId",
                        column: x => x.UsuarioEmpresaId,
                        principalTable: "UsuarioEmpresa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "MetodoPagamento",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    AssinanteId = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    EmpresaClienteId = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    Tipo = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TokenGateway = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GatewayProvider = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UltimosDigitos = table.Column<string>(type: "varchar(4)", maxLength: 4, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Bandeira = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NomeTitular = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataValidade = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Principal = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    Ativo = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    CriadoEm = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetodoPagamento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MetodoPagamento_Assinante_AssinanteId",
                        column: x => x.AssinanteId,
                        principalTable: "Assinante",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MetodoPagamento_EmpresaCliente_EmpresaClienteId",
                        column: x => x.EmpresaClienteId,
                        principalTable: "EmpresaCliente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "HistoricoNotificacao",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    CobrancaId = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    RegraCobrancaId = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    EmpresaClienteId = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    CanalUtilizado = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    MensagemEnviada = table.Column<string>(type: "TEXT", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MensagemErro = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataEnvio = table.Column<DateTime>(type: "DATETIME", nullable: false),
                    PayloadUtilizado = table.Column<string>(type: "TEXT", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RespostaProvedor = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CriadoEm = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoricoNotificacao", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistoricoNotificacao_Cobranca_CobrancaId",
                        column: x => x.CobrancaId,
                        principalTable: "Cobranca",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HistoricoNotificacao_EmpresaCliente_EmpresaClienteId",
                        column: x => x.EmpresaClienteId,
                        principalTable: "EmpresaCliente",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_HistoricoNotificacao_RegraCobranca_RegraCobrancaId",
                        column: x => x.RegraCobrancaId,
                        principalTable: "RegraCobranca",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Fatura",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    AssinanteId = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    EmpresaClienteId = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    NumeroFatura = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ValorBrutoCentavos = table.Column<long>(type: "bigint", nullable: false),
                    DescontoCentavos = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0"),
                    ImpostosCentavos = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "0"),
                    ValorLiquidoCentavos = table.Column<long>(type: "bigint", nullable: false),
                    DataEmissao = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DataVencimento = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DataPagamento = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Status = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false, defaultValueSql: "'Pendente'")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MetodoPagamentoId = table.Column<Guid>(type: "CHAR(36)", nullable: true, collation: "ascii_general_ci"),
                    TransacaoIdGateway = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Observacoes = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LinkBoleto = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    QrCodePix = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Moeda = table.Column<string>(type: "CHAR(3)", nullable: true, defaultValue: "BRL")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CriadoEm = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fatura", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Fatura_Assinante_AssinanteId",
                        column: x => x.AssinanteId,
                        principalTable: "Assinante",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Fatura_EmpresaCliente_EmpresaClienteId",
                        column: x => x.EmpresaClienteId,
                        principalTable: "EmpresaCliente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Fatura_MetodoPagamento_MetodoPagamentoId",
                        column: x => x.MetodoPagamentoId,
                        principalTable: "MetodoPagamento",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ItemFatura",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    FaturaId = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    Descricao = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Quantidade = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    ValorUnitarioCentavos = table.Column<long>(type: "bigint", nullable: false),
                    ValorTotalCentavos = table.Column<long>(type: "bigint", nullable: false),
                    TipoItem = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValueSql: "'Plano'")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PlanoOfertaId = table.Column<Guid>(type: "CHAR(36)", nullable: true, collation: "ascii_general_ci"),
                    CriadoEm = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemFatura", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemFatura_Fatura_FaturaId",
                        column: x => x.FaturaId,
                        principalTable: "Fatura",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItemFatura_PlanoOferta_PlanoOfertaId",
                        column: x => x.PlanoOfertaId,
                        principalTable: "PlanoOferta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TentativaPagamento",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    FaturaId = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    EmpresaClienteId = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    NumeroTentativa = table.Column<int>(type: "int", nullable: false),
                    DataTentativa = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Resultado = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CodigoErro = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MensagemErro = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TransacaoIdGateway = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GatewayProvider = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CriadoEm = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TentativaPagamento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TentativaPagamento_EmpresaCliente_EmpresaClienteId",
                        column: x => x.EmpresaClienteId,
                        principalTable: "EmpresaCliente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TentativaPagamento_Fatura_FaturaId",
                        column: x => x.FaturaId,
                        principalTable: "Fatura",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "idx_acao_tipo",
                table: "Acao",
                column: "TipoAcao");

            migrationBuilder.CreateIndex(
                name: "uk_acao_chave",
                table: "Acao",
                column: "Chave",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_plano",
                table: "Assinante",
                column: "PlanoOfertaId");

            migrationBuilder.CreateIndex(
                name: "idx_proxima_cobranca",
                table: "Assinante",
                column: "ProximaCobranca");

            migrationBuilder.CreateIndex(
                name: "idx_tenant_email",
                table: "Assinante",
                columns: new[] { "EmpresaClienteId", "Email" });

            migrationBuilder.CreateIndex(
                name: "idx_tenant_proxima_cobranca",
                table: "Assinante",
                columns: new[] { "EmpresaClienteId", "ProximaCobranca" });

            migrationBuilder.CreateIndex(
                name: "idx_tenant_status",
                table: "Assinante",
                columns: new[] { "EmpresaClienteId", "Status" });

            migrationBuilder.CreateIndex(
                name: "idx_cobranca_data_vencimento",
                table: "Cobranca",
                column: "DataVencimento");

            migrationBuilder.CreateIndex(
                name: "idx_cobranca_regra_status",
                table: "Cobranca",
                columns: new[] { "RegraCobrancaId", "Status" });

            migrationBuilder.CreateIndex(
                name: "idx_cobranca_status_tentativas",
                table: "Cobranca",
                columns: new[] { "Status", "TentativasEnvio" });

            migrationBuilder.CreateIndex(
                name: "idx_cobranca_tenant_status_disparo",
                table: "Cobranca",
                columns: new[] { "EmpresaClienteId", "Status", "DataDisparo" });

            migrationBuilder.CreateIndex(
                name: "idx_cnpj",
                table: "EmpresaCliente",
                column: "CNPJ",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_plano",
                table: "EmpresaCliente",
                column: "PlanoCobrioId");

            migrationBuilder.CreateIndex(
                name: "idx_status",
                table: "EmpresaCliente",
                column: "StatusContrato");

            migrationBuilder.CreateIndex(
                name: "idx_assinante_status",
                table: "Fatura",
                columns: new[] { "AssinanteId", "Status" });

            migrationBuilder.CreateIndex(
                name: "idx_data_vencimento",
                table: "Fatura",
                column: "DataVencimento");

            migrationBuilder.CreateIndex(
                name: "idx_status_vencimento",
                table: "Fatura",
                columns: new[] { "Status", "DataVencimento" });

            migrationBuilder.CreateIndex(
                name: "idx_tenant_status",
                table: "Fatura",
                columns: new[] { "EmpresaClienteId", "Status" });

            migrationBuilder.CreateIndex(
                name: "idx_tenant_vencimento",
                table: "Fatura",
                columns: new[] { "EmpresaClienteId", "DataVencimento" });

            migrationBuilder.CreateIndex(
                name: "IX_Fatura_MetodoPagamentoId",
                table: "Fatura",
                column: "MetodoPagamentoId");

            migrationBuilder.CreateIndex(
                name: "uk_numero_fatura",
                table: "Fatura",
                columns: new[] { "EmpresaClienteId", "NumeroFatura" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_historico_importacao_regra_data",
                table: "HistoricoImportacao",
                columns: new[] { "RegraCobrancaId", "DataImportacao" });

            migrationBuilder.CreateIndex(
                name: "idx_historico_importacao_status",
                table: "HistoricoImportacao",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "idx_historico_importacao_tenant_data",
                table: "HistoricoImportacao",
                columns: new[] { "EmpresaClienteId", "DataImportacao" });

            migrationBuilder.CreateIndex(
                name: "IX_HistoricoImportacao_UsuarioId",
                table: "HistoricoImportacao",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "idx_historico_canal_status",
                table: "HistoricoNotificacao",
                columns: new[] { "CanalUtilizado", "Status" });

            migrationBuilder.CreateIndex(
                name: "idx_historico_cobranca_status",
                table: "HistoricoNotificacao",
                columns: new[] { "CobrancaId", "Status" });

            migrationBuilder.CreateIndex(
                name: "idx_historico_regra_status",
                table: "HistoricoNotificacao",
                columns: new[] { "RegraCobrancaId", "Status" });

            migrationBuilder.CreateIndex(
                name: "idx_historico_status_data",
                table: "HistoricoNotificacao",
                columns: new[] { "Status", "DataEnvio" });

            migrationBuilder.CreateIndex(
                name: "idx_historico_tenant_data",
                table: "HistoricoNotificacao",
                columns: new[] { "EmpresaClienteId", "DataEnvio" });

            migrationBuilder.CreateIndex(
                name: "idx_fatura",
                table: "ItemFatura",
                column: "FaturaId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemFatura_PlanoOfertaId",
                table: "ItemFatura",
                column: "PlanoOfertaId");

            migrationBuilder.CreateIndex(
                name: "idx_assinante",
                table: "MetodoPagamento",
                column: "AssinanteId");

            migrationBuilder.CreateIndex(
                name: "idx_assinante_principal",
                table: "MetodoPagamento",
                columns: new[] { "AssinanteId", "Principal" });

            migrationBuilder.CreateIndex(
                name: "idx_tenant_ativo",
                table: "MetodoPagamento",
                columns: new[] { "EmpresaClienteId", "Ativo" });

            migrationBuilder.CreateIndex(
                name: "idx_modulo_ordem",
                table: "Modulo",
                column: "Ordem");

            migrationBuilder.CreateIndex(
                name: "uk_modulo_chave",
                table: "Modulo",
                column: "Chave",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_permissao_empresa_perfil",
                table: "PermissaoPerfil",
                columns: new[] { "EmpresaClienteId", "PerfilUsuario" });

            migrationBuilder.CreateIndex(
                name: "IX_PermissaoPerfil_AcaoId",
                table: "PermissaoPerfil",
                column: "AcaoId");

            migrationBuilder.CreateIndex(
                name: "IX_PermissaoPerfil_CriadoPorUsuarioId",
                table: "PermissaoPerfil",
                column: "CriadoPorUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_PermissaoPerfil_ModuloId",
                table: "PermissaoPerfil",
                column: "ModuloId");

            migrationBuilder.CreateIndex(
                name: "uk_permissao_empresa_perfil_modulo_acao",
                table: "PermissaoPerfil",
                columns: new[] { "EmpresaClienteId", "PerfilUsuario", "ModuloId", "AcaoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_tenant_ativo",
                table: "PlanoOferta",
                columns: new[] { "EmpresaClienteId", "Ativo" });

            migrationBuilder.CreateIndex(
                name: "idx_tenant_nome",
                table: "PlanoOferta",
                columns: new[] { "EmpresaClienteId", "Nome" });

            migrationBuilder.CreateIndex(
                name: "idx_token",
                table: "RefreshToken",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_usuario_ativo",
                table: "RefreshToken",
                columns: new[] { "UsuarioEmpresaId", "IsRevoked", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "idx_regra_cobranca_tenant_ativa",
                table: "RegraCobranca",
                columns: new[] { "EmpresaClienteId", "Ativa" });

            migrationBuilder.CreateIndex(
                name: "idx_regra_cobranca_tenant_canal",
                table: "RegraCobranca",
                columns: new[] { "EmpresaClienteId", "CanalNotificacao" });

            migrationBuilder.CreateIndex(
                name: "idx_regra_cobranca_token",
                table: "RegraCobranca",
                column: "TokenWebhook",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReguaDunningConfig_EmpresaClienteId",
                table: "ReguaDunningConfig",
                column: "EmpresaClienteId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_fatura",
                table: "TentativaPagamento",
                column: "FaturaId");

            migrationBuilder.CreateIndex(
                name: "idx_resultado",
                table: "TentativaPagamento",
                column: "Resultado");

            migrationBuilder.CreateIndex(
                name: "idx_tenant_data",
                table: "TentativaPagamento",
                columns: new[] { "EmpresaClienteId", "DataTentativa" });

            migrationBuilder.CreateIndex(
                name: "idx_tenant_ativo",
                table: "UsuarioEmpresa",
                columns: new[] { "EmpresaClienteId", "Ativo" });

            migrationBuilder.CreateIndex(
                name: "idx_tenant_email",
                table: "UsuarioEmpresa",
                columns: new[] { "EmpresaClienteId", "Email" });

            migrationBuilder.CreateIndex(
                name: "uk_email_empresa",
                table: "UsuarioEmpresa",
                columns: new[] { "Email", "EmpresaClienteId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HistoricoImportacao");

            migrationBuilder.DropTable(
                name: "HistoricoNotificacao");

            migrationBuilder.DropTable(
                name: "ItemFatura");

            migrationBuilder.DropTable(
                name: "PermissaoPerfil");

            migrationBuilder.DropTable(
                name: "RefreshToken");

            migrationBuilder.DropTable(
                name: "ReguaDunningConfig");

            migrationBuilder.DropTable(
                name: "TentativaPagamento");

            migrationBuilder.DropTable(
                name: "Cobranca");

            migrationBuilder.DropTable(
                name: "Acao");

            migrationBuilder.DropTable(
                name: "Modulo");

            migrationBuilder.DropTable(
                name: "UsuarioEmpresa");

            migrationBuilder.DropTable(
                name: "Fatura");

            migrationBuilder.DropTable(
                name: "RegraCobranca");

            migrationBuilder.DropTable(
                name: "MetodoPagamento");

            migrationBuilder.DropTable(
                name: "Assinante");

            migrationBuilder.DropTable(
                name: "PlanoOferta");

            migrationBuilder.DropTable(
                name: "EmpresaCliente");
        }
    }
}
