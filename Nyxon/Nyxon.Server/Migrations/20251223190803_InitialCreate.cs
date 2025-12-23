using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nyxon.Server.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Conversations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Version = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conversations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InviteCodes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CodeHash = table.Column<byte[]>(type: "bytea", nullable: false),
                    Used = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InviteCodes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Username = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<byte[]>(type: "bytea", nullable: false),
                    PasswordSalt = table.Column<byte[]>(type: "bytea", nullable: false),
                    PublicKey = table.Column<byte[]>(type: "bytea", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Version = table.Column<short>(type: "smallint", nullable: false),
                    Admin = table.Column<bool>(type: "boolean", nullable: false),
                    CanCreateInvites = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConversationUsers",
                columns: table => new
                {
                    ConversationId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsAdmin = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConversationUsers", x => new { x.ConversationId, x.UserId });
                    table.ForeignKey(
                        name: "FK_ConversationUsers_Conversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "Conversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConversationUsers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConversationVaults",
                columns: table => new
                {
                    ConversationId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Version = table.Column<short>(type: "smallint", nullable: false),
                    RecvCounter = table.Column<int>(type: "integer", nullable: false),
                    SendCounter = table.Column<int>(type: "integer", nullable: false),
                    VaultBlob = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConversationVaults", x => new { x.ConversationId, x.UserId });
                    table.ForeignKey(
                        name: "FK_ConversationVaults_Conversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "Conversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConversationVaults_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MessageMetadata",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ConversationId = table.Column<Guid>(type: "uuid", nullable: false),
                    KvKey = table.Column<string>(type: "text", nullable: false),
                    SenderId = table.Column<Guid>(type: "uuid", nullable: false),
                    RotationIndex = table.Column<int>(type: "integer", nullable: false),
                    MessageIndex = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Version = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageMetadata", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MessageMetadata_Conversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "Conversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MessageMetadata_Users_SenderId",
                        column: x => x.SenderId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OneTimePrekeys",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PublicKey = table.Column<byte[]>(type: "bytea", nullable: false),
                    EncryptedKey = table.Column<byte[]>(type: "bytea", nullable: false),
                    Used = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OneTimePrekeys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OneTimePrekeys_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SignedPrekeys",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PublicKey = table.Column<byte[]>(type: "bytea", nullable: false),
                    EncryptedKey = table.Column<byte[]>(type: "bytea", nullable: false),
                    Signature = table.Column<byte[]>(type: "bytea", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Version = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SignedPrekeys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SignedPrekeys_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserVaults",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Version = table.Column<short>(type: "smallint", nullable: false),
                    PassphraseSalt = table.Column<byte[]>(type: "bytea", nullable: false),
                    VaultKey = table.Column<byte[]>(type: "bytea", nullable: false),
                    PrivateIdentityKey = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserVaults", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_UserVaults_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Attachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ConversationId = table.Column<Guid>(type: "uuid", nullable: false),
                    MessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    StoragePtr = table.Column<string>(type: "text", nullable: false),
                    WrappedDek = table.Column<byte[]>(type: "bytea", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Version = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Attachments_Conversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "Conversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Attachments_MessageMetadata_MessageId",
                        column: x => x.MessageId,
                        principalTable: "MessageMetadata",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Attachments_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_ConversationId",
                table: "Attachments",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_MessageId",
                table: "Attachments",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_OwnerId",
                table: "Attachments",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_ConversationUsers_UserId",
                table: "ConversationUsers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ConversationVaults_UserId",
                table: "ConversationVaults",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageMetadata_ConversationId",
                table: "MessageMetadata",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageMetadata_SenderId",
                table: "MessageMetadata",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_OneTimePrekeys_UserId",
                table: "OneTimePrekeys",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SignedPrekeys_UserId",
                table: "SignedPrekeys",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Attachments");

            migrationBuilder.DropTable(
                name: "ConversationUsers");

            migrationBuilder.DropTable(
                name: "ConversationVaults");

            migrationBuilder.DropTable(
                name: "InviteCodes");

            migrationBuilder.DropTable(
                name: "OneTimePrekeys");

            migrationBuilder.DropTable(
                name: "SignedPrekeys");

            migrationBuilder.DropTable(
                name: "UserVaults");

            migrationBuilder.DropTable(
                name: "MessageMetadata");

            migrationBuilder.DropTable(
                name: "Conversations");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
