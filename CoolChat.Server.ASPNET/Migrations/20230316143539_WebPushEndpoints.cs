using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoolChat.Server.ASPNET.Migrations
{
    /// <inheritdoc />
    public partial class WebPushEndpoints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WebPushEndpoints",
                table: "Accounts",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WebPushEndpoints",
                table: "Accounts");
        }
    }
}
