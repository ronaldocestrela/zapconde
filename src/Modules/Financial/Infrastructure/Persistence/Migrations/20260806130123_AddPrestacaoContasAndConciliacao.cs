using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Modules.Financial.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPrestacaoContasAndConciliacao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConciliacoesBancarias",
                schema: "financial",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    ExtratoBancarioItemId = table.Column<int>(type: "integer", nullable: false),
                    OrigemTipo = table.Column<int>(type: "integer", nullable: false),
                    OrigemId = table.Column<int>(type: "integer", nullable: false),
                    DataConciliacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ConciliadoPorUserId = table.Column<int>(type: "integer", nullable: true),
                    Observacoes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConciliacoesBancarias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContasBancarias",
                schema: "financial",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    CondoId = table.Column<int>(type: "integer", nullable: false),
                    NomeBanco = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CodigoBanco = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Agencia = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    NumeroConta = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    TipoConta = table.Column<int>(type: "integer", nullable: false),
                    SaldoAtual = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Ativa = table.Column<bool>(type: "boolean", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContasBancarias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExtratoBancarioItens",
                schema: "financial",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    ContaBancariaId = table.Column<int>(type: "integer", nullable: false),
                    DataTransacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DescricaoHistorico = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    DocumentoRef = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TipoTransacao = table.Column<int>(type: "integer", nullable: false),
                    StatusConciliacao = table.Column<int>(type: "integer", nullable: false),
                    TransacaoConciliadaId = table.Column<int>(type: "integer", nullable: true),
                    ScoreConciliacao = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExtratoBancarioItens", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PastasDigitais",
                schema: "financial",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    CondoId = table.Column<int>(type: "integer", nullable: false),
                    Ano = table.Column<int>(type: "integer", nullable: false),
                    Mes = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataFechamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DataAprovacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AprovadoPorUserId = table.Column<int>(type: "integer", nullable: true),
                    ObservacoesConselho = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    ResumoExecutivoIa = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    SaldoAnterior = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalReceitas = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalDespesas = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    SaldoMes = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    SaldoAcumulado = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PastasDigitais", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DocumentosPrestacaoContas",
                schema: "financial",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    PastaDigitalId = table.Column<int>(type: "integer", nullable: false),
                    Categoria = table.Column<int>(type: "integer", nullable: false),
                    Titulo = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    NomeArquivo = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    UrlArquivo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TamanhoBytes = table.Column<long>(type: "bigint", nullable: false),
                    DataUpload = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UploadPorUserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentosPrestacaoContas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentosPrestacaoContas_PastasDigitais_PastaDigitalId",
                        column: x => x.PastaDigitalId,
                        principalSchema: "financial",
                        principalTable: "PastasDigitais",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ItensBalancete",
                schema: "financial",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    PastaDigitalId = table.Column<int>(type: "integer", nullable: false),
                    TipoLancamento = table.Column<int>(type: "integer", nullable: false),
                    Categoria = table.Column<int>(type: "integer", nullable: false),
                    Descricao = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    ValorOrcado = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ValorRealizado = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DataLancamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Conciliado = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItensBalancete", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItensBalancete_PastasDigitais_PastaDigitalId",
                        column: x => x.PastaDigitalId,
                        principalSchema: "financial",
                        principalTable: "PastasDigitais",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosPrestacaoContas_PastaDigitalId",
                schema: "financial",
                table: "DocumentosPrestacaoContas",
                column: "PastaDigitalId");

            migrationBuilder.CreateIndex(
                name: "IX_ItensBalancete_PastaDigitalId",
                schema: "financial",
                table: "ItensBalancete",
                column: "PastaDigitalId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConciliacoesBancarias",
                schema: "financial");

            migrationBuilder.DropTable(
                name: "ContasBancarias",
                schema: "financial");

            migrationBuilder.DropTable(
                name: "DocumentosPrestacaoContas",
                schema: "financial");

            migrationBuilder.DropTable(
                name: "ExtratoBancarioItens",
                schema: "financial");

            migrationBuilder.DropTable(
                name: "ItensBalancete",
                schema: "financial");

            migrationBuilder.DropTable(
                name: "PastasDigitais",
                schema: "financial");
        }
    }
}
