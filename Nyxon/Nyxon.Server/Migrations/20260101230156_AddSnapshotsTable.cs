using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nyxon.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddSnapshotsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RatchetSnapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConversationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    RotationIndex = table.Column<int>(type: "integer", nullable: false),
                    EncryptedSessionKey = table.Column<byte[]>(type: "bytea", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RatchetSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RatchetSnapshots_Conversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "Conversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RatchetSnapshots_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RatchetSnapshots_ConversationId",
                table: "RatchetSnapshots",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_RatchetSnapshots_UserId_ConversationId_Type_RotationIndex",
                table: "RatchetSnapshots",
                columns: new[] { "UserId", "ConversationId", "Type", "RotationIndex" })
                .Annotation("Npgsql:IndexInclude", new[] { "EncryptedSessionKey" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RatchetSnapshots");
        }
    }
}
