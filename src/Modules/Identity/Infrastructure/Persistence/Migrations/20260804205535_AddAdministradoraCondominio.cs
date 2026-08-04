using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Modules.Identity.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAdministradoraCondominio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "administradoras",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RazaoSocial = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Cnpj = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: false),
                    NomeFantasia = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    LicensePlan = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_administradoras", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "condominios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    Nome = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Tipo = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    TotalUnits = table.Column<int>(type: "integer", nullable: false),
                    NumberOfBlocks = table.Column<int>(type: "integer", nullable: false),
                    cep = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    logradouro = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    numero = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    bairro = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    cidade = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    uf = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    MasterAdminName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CorporateEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PhoneWhatsApp = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    EmergencyPhone = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    dia_vencimento = table.Column<int>(type: "integer", nullable: false),
                    juros_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    multa_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    bank_gateway = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    whatsapp_ai_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_condominios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_condominios_administradoras_TenantId",
                        column: x => x.TenantId,
                        principalTable: "administradoras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_administradoras_Cnpj",
                table: "administradoras",
                column: "Cnpj",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_condominios_TenantId_Nome",
                table: "condominios",
                columns: new[] { "TenantId", "Nome" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "condominios");

            migrationBuilder.DropTable(
                name: "administradoras");
        }
    }
}
