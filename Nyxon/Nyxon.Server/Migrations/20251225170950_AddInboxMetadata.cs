using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nyxon.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddInboxMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastRead",
                table: "ConversationUsers",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<short>(
                name: "Version",
                table: "ConversationUsers",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastMessageAt",
                table: "Conversations",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastRead",
                table: "ConversationUsers");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "ConversationUsers");

            migrationBuilder.DropColumn(
                name: "LastMessageAt",
                table: "Conversations");
        }
    }
}
