using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Modules.AccessControl.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEncomendaModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Encomendas",
                schema: "access_control",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    CondoId = table.Column<int>(type: "integer", nullable: false),
                    UnidadeId = table.Column<int>(type: "integer", nullable: false),
                    BlocoUnidade = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CodigoRastreio = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Remetente = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    Transportadora = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    LocalArmazenamento = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    DataRecebimento = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RecebidoPorNome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    DataRetirada = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RetiradoPorNome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    NotificadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Observacoes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CriadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Encomendas", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Encomendas_Tenant_CodigoRastreio",
                schema: "access_control",
                table: "Encomendas",
                columns: new[] { "TenantId", "CodigoRastreio" });

            migrationBuilder.CreateIndex(
                name: "IX_Encomendas_Tenant_Condo_Status",
                schema: "access_control",
                table: "Encomendas",
                columns: new[] { "TenantId", "CondoId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Encomendas_Tenant_Unidade",
                schema: "access_control",
                table: "Encomendas",
                columns: new[] { "TenantId", "UnidadeId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Encomendas",
                schema: "access_control");
        }
    }
}
