using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nyxon.Server.Migrations
{
    /// <inheritdoc />
    public partial class DeletionLogicChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ConversationUsers_UserId",
                table: "ConversationUsers");

            migrationBuilder.CreateIndex(
                name: "IX_MessageMetadata_KvKey",
                table: "MessageMetadata",
                column: "KvKey",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MessageMetadata_KvKey",
                table: "MessageMetadata");

            migrationBuilder.CreateIndex(
                name: "IX_ConversationUsers_UserId",
                table: "ConversationUsers",
                column: "UserId");
        }
    }
}
