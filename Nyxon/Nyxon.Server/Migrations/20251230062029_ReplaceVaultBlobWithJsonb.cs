using Microsoft.EntityFrameworkCore.Migrations;
using Nyxon.Core.Models.Vaults;

#nullable disable

namespace Nyxon.Server.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceVaultBlobWithJsonb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VaultBlob",
                table: "ConversationVaults");

            migrationBuilder.AddColumn<ConversationVaultData>(
                name: "VaultData",
                table: "ConversationVaults",
                type: "jsonb",
                nullable: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VaultData",
                table: "ConversationVaults");

            migrationBuilder.AddColumn<byte[]>(
                name: "VaultBlob",
                table: "ConversationVaults",
                type: "bytea",
                nullable: false,
                defaultValue: new byte[0]);
        }
    }
}
