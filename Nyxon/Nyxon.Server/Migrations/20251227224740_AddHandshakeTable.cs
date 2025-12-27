using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nyxon.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddHandshakeTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Handshakes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InitiatorId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConversationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Version = table.Column<short>(type: "smallint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SpkId = table.Column<Guid>(type: "uuid", nullable: false),
                    OpkId = table.Column<Guid>(type: "uuid", nullable: true),
                    PublicEphemeralKey = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Handshakes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Handshakes_Conversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "Conversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Handshakes_OneTimePrekeys_OpkId",
                        column: x => x.OpkId,
                        principalTable: "OneTimePrekeys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Handshakes_SignedPrekeys_SpkId",
                        column: x => x.SpkId,
                        principalTable: "SignedPrekeys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Handshakes_Users_InitiatorId",
                        column: x => x.InitiatorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Handshakes_ConversationId",
                table: "Handshakes",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_Handshakes_InitiatorId",
                table: "Handshakes",
                column: "InitiatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Handshakes_OpkId",
                table: "Handshakes",
                column: "OpkId");

            migrationBuilder.CreateIndex(
                name: "IX_Handshakes_SpkId",
                table: "Handshakes",
                column: "SpkId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Handshakes");
        }
    }
}
