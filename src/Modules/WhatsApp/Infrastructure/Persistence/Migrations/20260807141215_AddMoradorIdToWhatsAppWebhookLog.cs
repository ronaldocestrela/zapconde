using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Modules.WhatsApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMoradorIdToWhatsAppWebhookLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MoradorId",
                schema: "whatsapp",
                table: "WebhookLogs",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WebhookLogs_Tenant_MoradorId",
                schema: "whatsapp",
                table: "WebhookLogs",
                columns: new[] { "TenantId", "MoradorId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WebhookLogs_Tenant_MoradorId",
                schema: "whatsapp",
                table: "WebhookLogs");

            migrationBuilder.DropColumn(
                name: "MoradorId",
                schema: "whatsapp",
                table: "WebhookLogs");
        }
    }
}
