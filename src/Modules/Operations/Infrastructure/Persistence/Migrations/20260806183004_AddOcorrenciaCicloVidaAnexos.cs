using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Modules.Operations.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOcorrenciaCicloVidaAnexos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Ocorrencias",
                schema: "operations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    CondoId = table.Column<int>(type: "integer", nullable: false),
                    MoradorId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    MoradorNome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Titulo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Categoria = table.Column<int>(type: "integer", nullable: false),
                    Prioridade = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Localizacao = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    DataAbertura = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataConclusao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResponsavelId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ResponsavelNome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ObservacaoResolucao = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ocorrencias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AnexosOcorrencia",
                schema: "operations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    CondoId = table.Column<int>(type: "integer", nullable: false),
                    OcorrenciaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    NomeArquivo = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TamanhoBytes = table.Column<long>(type: "bigint", nullable: false),
                    DataUpload = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UploadPorUserId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnexosOcorrencia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnexosOcorrencia_Ocorrencias_OcorrenciaId",
                        column: x => x.OcorrenciaId,
                        principalSchema: "operations",
                        principalTable: "Ocorrencias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HistoricoOcorrencias",
                schema: "operations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    CondoId = table.Column<int>(type: "integer", nullable: false),
                    OcorrenciaId = table.Column<Guid>(type: "uuid", nullable: false),
                    StatusAnterior = table.Column<int>(type: "integer", nullable: true),
                    StatusNovo = table.Column<int>(type: "integer", nullable: false),
                    Comentario = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    DataAlteracao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AlteradoPorUserId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    AlteradoPorNome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoricoOcorrencias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistoricoOcorrencias_Ocorrencias_OcorrenciaId",
                        column: x => x.OcorrenciaId,
                        principalSchema: "operations",
                        principalTable: "Ocorrencias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnexosOcorrencia_OcorrenciaId",
                schema: "operations",
                table: "AnexosOcorrencia",
                column: "OcorrenciaId");

            migrationBuilder.CreateIndex(
                name: "IX_AnexosOcorrencia_TenantId_OcorrenciaId",
                schema: "operations",
                table: "AnexosOcorrencia",
                columns: new[] { "TenantId", "OcorrenciaId" });

            migrationBuilder.CreateIndex(
                name: "IX_HistoricoOcorrencias_OcorrenciaId",
                schema: "operations",
                table: "HistoricoOcorrencias",
                column: "OcorrenciaId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoricoOcorrencias_TenantId_OcorrenciaId",
                schema: "operations",
                table: "HistoricoOcorrencias",
                columns: new[] { "TenantId", "OcorrenciaId" });

            migrationBuilder.CreateIndex(
                name: "IX_Ocorrencias_TenantId_CondoId_Status",
                schema: "operations",
                table: "Ocorrencias",
                columns: new[] { "TenantId", "CondoId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Ocorrencias_TenantId_MoradorId",
                schema: "operations",
                table: "Ocorrencias",
                columns: new[] { "TenantId", "MoradorId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnexosOcorrencia",
                schema: "operations");

            migrationBuilder.DropTable(
                name: "HistoricoOcorrencias",
                schema: "operations");

            migrationBuilder.DropTable(
                name: "Ocorrencias",
                schema: "operations");
        }
    }
}
