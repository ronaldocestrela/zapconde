using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Modules.Financial.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialFinancial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "financial");

            migrationBuilder.CreateTable(
                name: "Faturas",
                schema: "financial",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    CondoId = table.Column<int>(type: "integer", nullable: false),
                    UnidadeId = table.Column<int>(type: "integer", nullable: false),
                    MoradorId = table.Column<int>(type: "integer", nullable: false),
                    Competencia = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    NumeroFatura = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DataEmissao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataVencimento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ValorOriginal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ValorDesconto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ValorMulta = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ValorJuros = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    DataPagamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Observacoes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Faturas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InboxState",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConsumerId = table.Column<Guid>(type: "uuid", nullable: false),
                    LockId = table.Column<Guid>(type: "uuid", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true),
                    Received = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReceiveCount = table.Column<int>(type: "integer", nullable: false),
                    ExpirationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Consumed = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Delivered = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastSequenceNumber = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InboxState", x => x.Id);
                    table.UniqueConstraint("AK_InboxState_MessageId_ConsumerId", x => new { x.MessageId, x.ConsumerId });
                });

            migrationBuilder.CreateTable(
                name: "OutboxState",
                columns: table => new
                {
                    OutboxId = table.Column<Guid>(type: "uuid", nullable: false),
                    LockId = table.Column<Guid>(type: "uuid", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Delivered = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastSequenceNumber = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxState", x => x.OutboxId);
                });

            migrationBuilder.CreateTable(
                name: "Boletos",
                schema: "financial",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    FaturaId = table.Column<int>(type: "integer", nullable: false),
                    NossoNumero = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LinhaDigitavel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CodigoBarras = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CodigoPixCopiaECola = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    QrCodeUrl = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    PixQrCodeBase64 = table.Column<string>(type: "text", nullable: false),
                    PdfUrl = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    ExternalChargeId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    GatewayProvider = table.Column<int>(type: "integer", nullable: false),
                    DataUltimaSincronizacaoGateway = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DataVencimento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataEmissao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataPagamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Boletos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Boletos_Faturas_FaturaId",
                        column: x => x.FaturaId,
                        principalSchema: "financial",
                        principalTable: "Faturas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ItensCobranca",
                schema: "financial",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    FaturaId = table.Column<int>(type: "integer", nullable: false),
                    Descricao = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    ValorUnitario = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Quantidade = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItensCobranca", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItensCobranca_Faturas_FaturaId",
                        column: x => x.FaturaId,
                        principalSchema: "financial",
                        principalTable: "Faturas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OutboxMessage",
                columns: table => new
                {
                    SequenceNumber = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EnqueueTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SentTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Headers = table.Column<string>(type: "text", nullable: true),
                    Properties = table.Column<string>(type: "text", nullable: true),
                    InboxMessageId = table.Column<Guid>(type: "uuid", nullable: true),
                    InboxConsumerId = table.Column<Guid>(type: "uuid", nullable: true),
                    OutboxId = table.Column<Guid>(type: "uuid", nullable: true),
                    MessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContentType = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    MessageType = table.Column<string>(type: "text", nullable: false),
                    Body = table.Column<string>(type: "text", nullable: false),
                    ConversationId = table.Column<Guid>(type: "uuid", nullable: true),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: true),
                    InitiatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    SourceAddress = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    DestinationAddress = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ResponseAddress = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    FaultAddress = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ExpirationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessage", x => x.SequenceNumber);
                    table.ForeignKey(
                        name: "FK_OutboxMessage_InboxState_InboxMessageId_InboxConsumerId",
                        columns: x => new { x.InboxMessageId, x.InboxConsumerId },
                        principalTable: "InboxState",
                        principalColumns: new[] { "MessageId", "ConsumerId" });
                    table.ForeignKey(
                        name: "FK_OutboxMessage_OutboxState_OutboxId",
                        column: x => x.OutboxId,
                        principalTable: "OutboxState",
                        principalColumn: "OutboxId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Boletos_FaturaId",
                schema: "financial",
                table: "Boletos",
                column: "FaturaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Boletos_TenantId_ExternalChargeId",
                schema: "financial",
                table: "Boletos",
                columns: new[] { "TenantId", "ExternalChargeId" });

            migrationBuilder.CreateIndex(
                name: "IX_Boletos_TenantId_FaturaId",
                schema: "financial",
                table: "Boletos",
                columns: new[] { "TenantId", "FaturaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Boletos_TenantId_NossoNumero",
                schema: "financial",
                table: "Boletos",
                columns: new[] { "TenantId", "NossoNumero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Faturas_TenantId_CondoId_Competencia",
                schema: "financial",
                table: "Faturas",
                columns: new[] { "TenantId", "CondoId", "Competencia" });

            migrationBuilder.CreateIndex(
                name: "IX_Faturas_TenantId_NumeroFatura",
                schema: "financial",
                table: "Faturas",
                columns: new[] { "TenantId", "NumeroFatura" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Faturas_TenantId_UnidadeId",
                schema: "financial",
                table: "Faturas",
                columns: new[] { "TenantId", "UnidadeId" });

            migrationBuilder.CreateIndex(
                name: "IX_InboxState_Delivered",
                table: "InboxState",
                column: "Delivered");

            migrationBuilder.CreateIndex(
                name: "IX_ItensCobranca_FaturaId",
                schema: "financial",
                table: "ItensCobranca",
                column: "FaturaId");

            migrationBuilder.CreateIndex(
                name: "IX_ItensCobranca_TenantId_FaturaId",
                schema: "financial",
                table: "ItensCobranca",
                columns: new[] { "TenantId", "FaturaId" });

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessage_EnqueueTime",
                table: "OutboxMessage",
                column: "EnqueueTime");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessage_ExpirationTime",
                table: "OutboxMessage",
                column: "ExpirationTime");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessage_InboxMessageId_InboxConsumerId_SequenceNumber",
                table: "OutboxMessage",
                columns: new[] { "InboxMessageId", "InboxConsumerId", "SequenceNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessage_OutboxId_SequenceNumber",
                table: "OutboxMessage",
                columns: new[] { "OutboxId", "SequenceNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OutboxState_Created",
                table: "OutboxState",
                column: "Created");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Boletos",
                schema: "financial");

            migrationBuilder.DropTable(
                name: "ItensCobranca",
                schema: "financial");

            migrationBuilder.DropTable(
                name: "OutboxMessage");

            migrationBuilder.DropTable(
                name: "Faturas",
                schema: "financial");

            migrationBuilder.DropTable(
                name: "InboxState");

            migrationBuilder.DropTable(
                name: "OutboxState");
        }
    }
}
