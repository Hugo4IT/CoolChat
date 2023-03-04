using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoolChat.Server.ASPNET.Migrations
{
    /// <inheritdoc />
    public partial class IconResource : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IconUrl",
                table: "Groups");

            migrationBuilder.AddColumn<int>(
                name: "IconId",
                table: "Groups",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Groups_IconId",
                table: "Groups",
                column: "IconId");

            migrationBuilder.AddForeignKey(
                name: "FK_Groups_Resources_IconId",
                table: "Groups",
                column: "IconId",
                principalTable: "Resources",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Groups_Resources_IconId",
                table: "Groups");

            migrationBuilder.DropIndex(
                name: "IX_Groups_IconId",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "IconId",
                table: "Groups");

            migrationBuilder.AddColumn<string>(
                name: "IconUrl",
                table: "Groups",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
