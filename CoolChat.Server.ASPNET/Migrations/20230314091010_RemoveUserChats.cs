using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoolChat.Server.ASPNET.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUserChats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountChat");

            migrationBuilder.AddColumn<int>(
                name: "ChatId",
                table: "Accounts",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_ChatId",
                table: "Accounts",
                column: "ChatId");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_Chats_ChatId",
                table: "Accounts",
                column: "ChatId",
                principalTable: "Chats",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_Chats_ChatId",
                table: "Accounts");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_ChatId",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "ChatId",
                table: "Accounts");

            migrationBuilder.CreateTable(
                name: "AccountChat",
                columns: table => new
                {
                    ChatsId = table.Column<int>(type: "INTEGER", nullable: false),
                    MembersId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountChat", x => new { x.ChatsId, x.MembersId });
                    table.ForeignKey(
                        name: "FK_AccountChat_Accounts_MembersId",
                        column: x => x.MembersId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccountChat_Chats_ChatsId",
                        column: x => x.ChatsId,
                        principalTable: "Chats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountChat_MembersId",
                table: "AccountChat",
                column: "MembersId");
        }
    }
}
