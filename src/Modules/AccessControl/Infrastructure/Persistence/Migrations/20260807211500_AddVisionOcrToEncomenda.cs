using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Modules.AccessControl.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVisionOcrToEncomenda : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "ConfiancaOcr",
                schema: "access_control",
                table: "Encomendas",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DadosOcrJson",
                schema: "access_control",
                table: "Encomendas",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FotoEtiquetaUrl",
                schema: "access_control",
                table: "Encomendas",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Encomendas_Tenant_Status_ConfiancaOcr",
                schema: "access_control",
                table: "Encomendas",
                columns: new[] { "TenantId", "Status", "ConfiancaOcr" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Encomendas_Tenant_Status_ConfiancaOcr",
                schema: "access_control",
                table: "Encomendas");

            migrationBuilder.DropColumn(
                name: "ConfiancaOcr",
                schema: "access_control",
                table: "Encomendas");

            migrationBuilder.DropColumn(
                name: "DadosOcrJson",
                schema: "access_control",
                table: "Encomendas");

            migrationBuilder.DropColumn(
                name: "FotoEtiquetaUrl",
                schema: "access_control",
                table: "Encomendas");
        }
    }
}
