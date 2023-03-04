using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoolChat.Server.ASPNET.Migrations
{
    /// <inheritdoc />
    public partial class MemberLists : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountGroup");

            migrationBuilder.AddColumn<int>(
                name: "AccountId",
                table: "Groups",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MemberListId",
                table: "Groups",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MemberListId",
                table: "Chats",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MemberListId",
                table: "Accounts",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MemberLists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberLists", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Groups_AccountId",
                table: "Groups",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Groups_MemberListId",
                table: "Groups",
                column: "MemberListId");

            migrationBuilder.CreateIndex(
                name: "IX_Chats_MemberListId",
                table: "Chats",
                column: "MemberListId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_MemberListId",
                table: "Accounts",
                column: "MemberListId");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_MemberLists_MemberListId",
                table: "Accounts",
                column: "MemberListId",
                principalTable: "MemberLists",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Chats_MemberLists_MemberListId",
                table: "Chats",
                column: "MemberListId",
                principalTable: "MemberLists",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Groups_Accounts_AccountId",
                table: "Groups",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Groups_MemberLists_MemberListId",
                table: "Groups",
                column: "MemberListId",
                principalTable: "MemberLists",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_MemberLists_MemberListId",
                table: "Accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_Chats_MemberLists_MemberListId",
                table: "Chats");

            migrationBuilder.DropForeignKey(
                name: "FK_Groups_Accounts_AccountId",
                table: "Groups");

            migrationBuilder.DropForeignKey(
                name: "FK_Groups_MemberLists_MemberListId",
                table: "Groups");

            migrationBuilder.DropTable(
                name: "MemberLists");

            migrationBuilder.DropIndex(
                name: "IX_Groups_AccountId",
                table: "Groups");

            migrationBuilder.DropIndex(
                name: "IX_Groups_MemberListId",
                table: "Groups");

            migrationBuilder.DropIndex(
                name: "IX_Chats_MemberListId",
                table: "Chats");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_MemberListId",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "MemberListId",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "MemberListId",
                table: "Chats");

            migrationBuilder.DropColumn(
                name: "MemberListId",
                table: "Accounts");

            migrationBuilder.CreateTable(
                name: "AccountGroup",
                columns: table => new
                {
                    GroupsId = table.Column<int>(type: "INTEGER", nullable: false),
                    MembersId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountGroup", x => new { x.GroupsId, x.MembersId });
                    table.ForeignKey(
                        name: "FK_AccountGroup_Accounts_MembersId",
                        column: x => x.MembersId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccountGroup_Groups_GroupsId",
                        column: x => x.GroupsId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountGroup_MembersId",
                table: "AccountGroup",
                column: "MembersId");
        }
    }
}
