using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Modules.Operations.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAssembleiaVirtualVotacaoAta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AssembleiasVirtuais",
                schema: "operations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    CondoId = table.Column<int>(type: "integer", nullable: false),
                    Titulo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    DataInicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataFim = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataEncerramento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AtaTexto = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    AtaGeradaEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CriadoPorUserId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssembleiasVirtuais", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PautasAssembleia",
                schema: "operations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AssembleiaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Titulo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Ordem = table.Column<int>(type: "integer", nullable: false),
                    TipoVotacao = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    OpcoesDisponiveis = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PautasAssembleia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PautasAssembleia_AssembleiasVirtuais_AssembleiaId",
                        column: x => x.AssembleiaId,
                        principalSchema: "operations",
                        principalTable: "AssembleiasVirtuais",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VotosAssembleia",
                schema: "operations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    CondoId = table.Column<int>(type: "integer", nullable: false),
                    AssembleiaId = table.Column<Guid>(type: "uuid", nullable: false),
                    PautaId = table.Column<Guid>(type: "uuid", nullable: false),
                    MoradorUserId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UnidadeId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    OpcaoEscolhida = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PesoVoto = table.Column<double>(type: "double precision", nullable: false, defaultValue: 1.0),
                    DataVoto = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VotosAssembleia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VotosAssembleia_PautasAssembleia_PautaId",
                        column: x => x.PautaId,
                        principalSchema: "operations",
                        principalTable: "PautasAssembleia",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssembleiasVirtuais_TenantId_CondoId_Status",
                schema: "operations",
                table: "AssembleiasVirtuais",
                columns: new[] { "TenantId", "CondoId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AssembleiasVirtuais_TenantId_DataInicio_DataFim",
                schema: "operations",
                table: "AssembleiasVirtuais",
                columns: new[] { "TenantId", "DataInicio", "DataFim" });

            migrationBuilder.CreateIndex(
                name: "IX_PautasAssembleia_AssembleiaId",
                schema: "operations",
                table: "PautasAssembleia",
                column: "AssembleiaId");

            migrationBuilder.CreateIndex(
                name: "IX_VotosAssembleia_AssembleiaId",
                schema: "operations",
                table: "VotosAssembleia",
                column: "AssembleiaId");

            migrationBuilder.CreateIndex(
                name: "IX_VotosAssembleia_PautaId",
                schema: "operations",
                table: "VotosAssembleia",
                column: "PautaId");

            migrationBuilder.CreateIndex(
                name: "IX_VotosAssembleia_TenantId_PautaId_UnidadeId",
                schema: "operations",
                table: "VotosAssembleia",
                columns: new[] { "TenantId", "PautaId", "UnidadeId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VotosAssembleia",
                schema: "operations");

            migrationBuilder.DropTable(
                name: "PautasAssembleia",
                schema: "operations");

            migrationBuilder.DropTable(
                name: "AssembleiasVirtuais",
                schema: "operations");
        }
    }
}
