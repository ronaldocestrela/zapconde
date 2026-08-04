using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Modules.Identity.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMoradorPhoneVerification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PhoneVerificationRequestedAtUtc",
                table: "moradores",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneVerificationStatus",
                table: "moradores",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "NaoInformado");

            migrationBuilder.AddColumn<DateTime>(
                name: "PhoneVerifiedAtUtc",
                table: "moradores",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TelefoneWhatsAppE164",
                table: "moradores",
                type: "character varying(14)",
                maxLength: 14,
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE moradores
                SET "TelefoneWhatsAppE164" =
                    CASE
                        WHEN regexp_replace("TelefoneWhatsApp", '[^0-9]', '', 'g') ~ '^55[1-9][0-9]9[0-9]{8}$'
                            THEN '+' || regexp_replace("TelefoneWhatsApp", '[^0-9]', '', 'g')
                        WHEN regexp_replace("TelefoneWhatsApp", '[^0-9]', '', 'g') ~ '^[1-9][0-9]9[0-9]{8}$'
                            THEN '+55' || regexp_replace("TelefoneWhatsApp", '[^0-9]', '', 'g')
                        ELSE NULL
                    END,
                    "PhoneVerificationStatus" = 'NaoInformado';
                """);

            migrationBuilder.CreateIndex(
                name: "IX_moradores_TenantId_CondoId_TelefoneWhatsAppE164",
                table: "moradores",
                columns: new[] { "TenantId", "CondoId", "TelefoneWhatsAppE164" },
                unique: true,
                filter: "\"PhoneVerificationStatus\" = 'Validado' AND \"TelefoneWhatsAppE164\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_moradores_TenantId_CondoId_TelefoneWhatsAppE164",
                table: "moradores");

            migrationBuilder.DropColumn(
                name: "PhoneVerificationRequestedAtUtc",
                table: "moradores");

            migrationBuilder.DropColumn(
                name: "PhoneVerificationStatus",
                table: "moradores");

            migrationBuilder.DropColumn(
                name: "PhoneVerifiedAtUtc",
                table: "moradores");

            migrationBuilder.DropColumn(
                name: "TelefoneWhatsAppE164",
                table: "moradores");
        }
    }
}
