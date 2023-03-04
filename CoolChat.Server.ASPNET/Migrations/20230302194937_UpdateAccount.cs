using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoolChat.Server.ASPNET.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Groups_Accounts_AccountId",
                table: "Groups");

            migrationBuilder.DropIndex(
                name: "IX_Groups_AccountId",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "Groups");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AccountId",
                table: "Groups",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Groups_AccountId",
                table: "Groups",
                column: "AccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_Groups_Accounts_AccountId",
                table: "Groups",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id");
        }
    }
}
