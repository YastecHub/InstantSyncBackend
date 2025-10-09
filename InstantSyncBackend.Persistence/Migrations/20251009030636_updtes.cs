using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InstantSyncBackend.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class updtes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BankReferenceNumber",
                table: "Transactions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FailedAt",
                table: "Transactions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FailureReason",
                table: "Transactions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "InitiatedAt",
                table: "Transactions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PendingAt",
                table: "Transactions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResponseCode",
                table: "Transactions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RetryCount",
                table: "Transactions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "SentAt",
                table: "Transactions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SessionId",
                table: "Transactions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StatusMessage",
                table: "Transactions",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TransactionQueues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TransactionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrentStatus = table.Column<int>(type: "integer", nullable: false),
                    NextStatus = table.Column<int>(type: "integer", nullable: false),
                    ScheduledProcessTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    RetryCount = table.Column<int>(type: "integer", nullable: false),
                    MaxRetries = table.Column<int>(type: "integer", nullable: false),
                    IsProcessing = table.Column<bool>(type: "boolean", nullable: false),
                    ProcessingNode = table.Column<string>(type: "text", nullable: true),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    QueueType = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionQueues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransactionQueues_Transactions_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_TransactionReference",
                table: "Transactions",
                column: "TransactionReference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransactionQueues_IsProcessing_ScheduledProcessTime_Priority",
                table: "TransactionQueues",
                columns: new[] { "IsProcessing", "ScheduledProcessTime", "Priority" });

            migrationBuilder.CreateIndex(
                name: "IX_TransactionQueues_TransactionId",
                table: "TransactionQueues",
                column: "TransactionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TransactionQueues");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_TransactionReference",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "BankReferenceNumber",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "FailedAt",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "FailureReason",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "InitiatedAt",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "PendingAt",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "ResponseCode",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "RetryCount",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "SentAt",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "SessionId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "StatusMessage",
                table: "Transactions");
        }
    }
}
