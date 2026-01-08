using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nyxon.Server.Migrations
{
    /// <inheritdoc />
    public partial class ConversationVaultRotationIndexUniqueIndexFix2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RatchetSnapshots_RotationIndex_Type",
                table: "RatchetSnapshots");

            migrationBuilder.DropIndex(
                name: "IX_RatchetSnapshots_UserId_ConversationId_Type_RotationIndex",
                table: "RatchetSnapshots");

            migrationBuilder.CreateIndex(
                name: "IX_RatchetSnapshots_UserId_ConversationId_Type_RotationIndex",
                table: "RatchetSnapshots",
                columns: new[] { "UserId", "ConversationId", "Type", "RotationIndex" },
                unique: true)
                .Annotation("Npgsql:IndexInclude", new[] { "EncryptedSessionKey" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RatchetSnapshots_UserId_ConversationId_Type_RotationIndex",
                table: "RatchetSnapshots");

            migrationBuilder.CreateIndex(
                name: "IX_RatchetSnapshots_RotationIndex_Type",
                table: "RatchetSnapshots",
                columns: new[] { "RotationIndex", "Type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RatchetSnapshots_UserId_ConversationId_Type_RotationIndex",
                table: "RatchetSnapshots",
                columns: new[] { "UserId", "ConversationId", "Type", "RotationIndex" })
                .Annotation("Npgsql:IndexInclude", new[] { "EncryptedSessionKey" });
        }
    }
}
