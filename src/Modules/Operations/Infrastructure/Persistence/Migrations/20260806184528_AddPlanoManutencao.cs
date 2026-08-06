using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Modules.Operations.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPlanoManutencao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlanosManutencao",
                schema: "operations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    CondoId = table.Column<int>(type: "integer", nullable: false),
                    Titulo = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Categoria = table.Column<int>(type: "integer", nullable: false),
                    Periodicidade = table.Column<int>(type: "integer", nullable: false),
                    DataUltimaManutencao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DataProximaManutencao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ResponsavelTecnico = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    EmpresaContratada = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    CustoEstimado = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    CustoReal = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    Observacoes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanosManutencao", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlanosManutencao_TenantId_CondoId_Status",
                schema: "operations",
                table: "PlanosManutencao",
                columns: new[] { "TenantId", "CondoId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_PlanosManutencao_TenantId_DataProximaManutencao",
                schema: "operations",
                table: "PlanosManutencao",
                columns: new[] { "TenantId", "DataProximaManutencao" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlanosManutencao",
                schema: "operations");
        }
    }
}
