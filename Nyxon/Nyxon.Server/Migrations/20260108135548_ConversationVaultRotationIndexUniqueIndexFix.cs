using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nyxon.Server.Migrations
{
    /// <inheritdoc />
    public partial class ConversationVaultRotationIndexUniqueIndexFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RatchetSnapshots_RotationIndex",
                table: "RatchetSnapshots");

            migrationBuilder.CreateIndex(
                name: "IX_RatchetSnapshots_RotationIndex_Type",
                table: "RatchetSnapshots",
                columns: new[] { "RotationIndex", "Type" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RatchetSnapshots_RotationIndex_Type",
                table: "RatchetSnapshots");

            migrationBuilder.CreateIndex(
                name: "IX_RatchetSnapshots_RotationIndex",
                table: "RatchetSnapshots",
                column: "RotationIndex",
                unique: true);
        }
    }
}
