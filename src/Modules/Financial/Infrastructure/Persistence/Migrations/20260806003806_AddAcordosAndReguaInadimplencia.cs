using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Modules.Financial.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAcordosAndReguaInadimplencia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Acordos",
                schema: "financial",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    CondoId = table.Column<int>(type: "integer", nullable: false),
                    UnidadeId = table.Column<int>(type: "integer", nullable: false),
                    MoradorId = table.Column<int>(type: "integer", nullable: false),
                    NumeroAcordo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataAceite = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DataPrimeiroVencimento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ValorTotalOriginal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ValorDesconto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ValorTotalAcordo = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    QuantidadeParcelas = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Observacoes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Acordos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EtapasReguaInadimplencia",
                schema: "financial",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    CondoId = table.Column<int>(type: "integer", nullable: false),
                    Ordem = table.Column<int>(type: "integer", nullable: false),
                    DiasAtrasoMinimo = table.Column<int>(type: "integer", nullable: false),
                    DiasAtrasoMaximo = table.Column<int>(type: "integer", nullable: false),
                    NomeEtapa = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Canal = table.Column<int>(type: "integer", nullable: false),
                    TipoAcao = table.Column<int>(type: "integer", nullable: false),
                    TemplateMensagem = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EtapasReguaInadimplencia", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HistoricosCobranca",
                schema: "financial",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    CondoId = table.Column<int>(type: "integer", nullable: false),
                    UnidadeId = table.Column<int>(type: "integer", nullable: false),
                    MoradorId = table.Column<int>(type: "integer", nullable: false),
                    FaturaId = table.Column<int>(type: "integer", nullable: false),
                    EtapaReguaId = table.Column<int>(type: "integer", nullable: false),
                    DataExecucao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Canal = table.Column<int>(type: "integer", nullable: false),
                    TipoAcao = table.Column<int>(type: "integer", nullable: false),
                    MensagemEnviada = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Sucesso = table.Column<bool>(type: "boolean", nullable: false),
                    Observacao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoricosCobranca", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AcordoFaturasVinculadas",
                schema: "financial",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    AcordoId = table.Column<int>(type: "integer", nullable: false),
                    FaturaId = table.Column<int>(type: "integer", nullable: false),
                    ValorFaturaOriginal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AcordoFaturasVinculadas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AcordoFaturasVinculadas_Acordos_AcordoId",
                        column: x => x.AcordoId,
                        principalSchema: "financial",
                        principalTable: "Acordos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ParcelasAcordo",
                schema: "financial",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    AcordoId = table.Column<int>(type: "integer", nullable: false),
                    NumeroParcela = table.Column<int>(type: "integer", nullable: false),
                    DataVencimento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ValorParcela = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    DataPagamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FaturaGeradaId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParcelasAcordo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParcelasAcordo_Acordos_AcordoId",
                        column: x => x.AcordoId,
                        principalSchema: "financial",
                        principalTable: "Acordos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AcordoFaturasVinculadas_AcordoId",
                schema: "financial",
                table: "AcordoFaturasVinculadas",
                column: "AcordoId");

            migrationBuilder.CreateIndex(
                name: "IX_ParcelasAcordo_AcordoId",
                schema: "financial",
                table: "ParcelasAcordo",
                column: "AcordoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AcordoFaturasVinculadas",
                schema: "financial");

            migrationBuilder.DropTable(
                name: "EtapasReguaInadimplencia",
                schema: "financial");

            migrationBuilder.DropTable(
                name: "HistoricosCobranca",
                schema: "financial");

            migrationBuilder.DropTable(
                name: "ParcelasAcordo",
                schema: "financial");

            migrationBuilder.DropTable(
                name: "Acordos",
                schema: "financial");
        }
    }
}
