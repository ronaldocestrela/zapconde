using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Modules.Operations.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddIaTriagemToOcorrencia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OrigemTriagemIa",
                schema: "operations",
                table: "Ocorrencias",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResumoTriagemIa",
                schema: "operations",
                table: "Ocorrencias",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ConfiancaTriagemIa",
                schema: "operations",
                table: "Ocorrencias",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AudioUrl",
                schema: "operations",
                table: "Ocorrencias",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TranscricaoAudio",
                schema: "operations",
                table: "Ocorrencias",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SetorResponsavelSugerido",
                schema: "operations",
                table: "Ocorrencias",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrigemTriagemIa",
                schema: "operations",
                table: "Ocorrencias");

            migrationBuilder.DropColumn(
                name: "ResumoTriagemIa",
                schema: "operations",
                table: "Ocorrencias");

            migrationBuilder.DropColumn(
                name: "ConfiancaTriagemIa",
                schema: "operations",
                table: "Ocorrencias");

            migrationBuilder.DropColumn(
                name: "AudioUrl",
                schema: "operations",
                table: "Ocorrencias");

            migrationBuilder.DropColumn(
                name: "TranscricaoAudio",
                schema: "operations",
                table: "Ocorrencias");

            migrationBuilder.DropColumn(
                name: "SetorResponsavelSugerido",
                schema: "operations",
                table: "Ocorrencias");
        }
    }
}
