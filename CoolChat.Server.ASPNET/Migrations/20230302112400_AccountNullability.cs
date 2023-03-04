using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoolChat.Server.ASPNET.Migrations
{
    /// <inheritdoc />
    public partial class AccountNullability : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_AccountSettings_SettingsId",
                table: "Accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_Profiles_ProfileId",
                table: "Accounts");

            migrationBuilder.AlterColumn<int>(
                name: "SettingsId",
                table: "Accounts",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ProfileId",
                table: "Accounts",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_AccountSettings_SettingsId",
                table: "Accounts",
                column: "SettingsId",
                principalTable: "AccountSettings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_Profiles_ProfileId",
                table: "Accounts",
                column: "ProfileId",
                principalTable: "Profiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_AccountSettings_SettingsId",
                table: "Accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_Profiles_ProfileId",
                table: "Accounts");

            migrationBuilder.AlterColumn<int>(
                name: "SettingsId",
                table: "Accounts",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<int>(
                name: "ProfileId",
                table: "Accounts",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_AccountSettings_SettingsId",
                table: "Accounts",
                column: "SettingsId",
                principalTable: "AccountSettings",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_Profiles_ProfileId",
                table: "Accounts",
                column: "ProfileId",
                principalTable: "Profiles",
                principalColumn: "Id");
        }
    }
}
