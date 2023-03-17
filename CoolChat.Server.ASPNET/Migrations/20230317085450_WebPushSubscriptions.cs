using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoolChat.Server.ASPNET.Migrations
{
    /// <inheritdoc />
    public partial class WebPushSubscriptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WebPushEndpoints",
                table: "Accounts");

            migrationBuilder.CreateTable(
                name: "WebPushSubscriptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Endpoint = table.Column<string>(type: "TEXT", nullable: false),
                    Key_p256dh = table.Column<string>(type: "TEXT", nullable: false),
                    Key_auth = table.Column<string>(type: "TEXT", nullable: false),
                    AccountId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebPushSubscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WebPushSubscriptions_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_WebPushSubscriptions_AccountId",
                table: "WebPushSubscriptions",
                column: "AccountId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WebPushSubscriptions");

            migrationBuilder.AddColumn<string>(
                name: "WebPushEndpoints",
                table: "Accounts",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
