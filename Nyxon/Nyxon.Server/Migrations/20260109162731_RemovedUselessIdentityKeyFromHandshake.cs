using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nyxon.Server.Migrations
{
    /// <inheritdoc />
    public partial class RemovedUselessIdentityKeyFromHandshake : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PublicIdentityKey",
                table: "Handshakes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "PublicIdentityKey",
                table: "Handshakes",
                type: "bytea",
                nullable: false,
                defaultValue: new byte[0]);
        }
    }
}
