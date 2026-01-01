using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nyxon.Server.Migrations
{
    /// <inheritdoc />
    public partial class MinorChangesAndIndexing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Handshakes_OneTimePrekeys_OpkId",
                table: "Handshakes");

            migrationBuilder.AddColumn<byte[]>(
                name: "PublicIdentityKey",
                table: "Handshakes",
                type: "bytea",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<Guid>(
                name: "TargetUserId",
                table: "Handshakes",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Handshakes_ExpiresAt",
                table: "Handshakes",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_Handshakes_TargetUserId_CreatedAt",
                table: "Handshakes",
                columns: new[] { "TargetUserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ConversationUsers_UserId_ConversationId",
                table: "ConversationUsers",
                columns: new[] { "UserId", "ConversationId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Handshakes_OneTimePrekeys_OpkId",
                table: "Handshakes",
                column: "OpkId",
                principalTable: "OneTimePrekeys",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Handshakes_Users_TargetUserId",
                table: "Handshakes",
                column: "TargetUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Handshakes_OneTimePrekeys_OpkId",
                table: "Handshakes");

            migrationBuilder.DropForeignKey(
                name: "FK_Handshakes_Users_TargetUserId",
                table: "Handshakes");

            migrationBuilder.DropIndex(
                name: "IX_Handshakes_ExpiresAt",
                table: "Handshakes");

            migrationBuilder.DropIndex(
                name: "IX_Handshakes_TargetUserId_CreatedAt",
                table: "Handshakes");

            migrationBuilder.DropIndex(
                name: "IX_ConversationUsers_UserId_ConversationId",
                table: "ConversationUsers");

            migrationBuilder.DropColumn(
                name: "PublicIdentityKey",
                table: "Handshakes");

            migrationBuilder.DropColumn(
                name: "TargetUserId",
                table: "Handshakes");

            migrationBuilder.AddForeignKey(
                name: "FK_Handshakes_OneTimePrekeys_OpkId",
                table: "Handshakes",
                column: "OpkId",
                principalTable: "OneTimePrekeys",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
