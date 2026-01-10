using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nyxon.Server.Migrations
{
    /// <inheritdoc />
    public partial class AgreementKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PublicKey",
                table: "Users",
                newName: "PublicIdentityKey");

            migrationBuilder.AddColumn<byte[]>(
                name: "PrivateAgreementKey",
                table: "UserVaults",
                type: "bytea",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<byte[]>(
                name: "PublicAgreementKey",
                table: "Users",
                type: "bytea",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<byte[]>(
                name: "PublicAgreementKey",
                table: "Handshakes",
                type: "bytea",
                nullable: false,
                defaultValue: new byte[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrivateAgreementKey",
                table: "UserVaults");

            migrationBuilder.DropColumn(
                name: "PublicAgreementKey",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PublicAgreementKey",
                table: "Handshakes");

            migrationBuilder.RenameColumn(
                name: "PublicIdentityKey",
                table: "Users",
                newName: "PublicKey");
        }
    }
}
