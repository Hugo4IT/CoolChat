using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoolChat.Server.ASPNET.Migrations
{
    /// <inheritdoc />
    public partial class Roles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OwnerId",
                table: "Groups",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SettingsId",
                table: "Groups",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsRestricted",
                table: "Channels",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "GroupSettingsId",
                table: "Accounts",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GroupSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PrimaryColor = table.Column<string>(type: "TEXT", nullable: false),
                    Public = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    CanEditOtherUsersMessages = table.Column<bool>(type: "INTEGER", nullable: false),
                    GroupSettingsId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RolePermissions_GroupSettings_GroupSettingsId",
                        column: x => x.GroupSettingsId,
                        principalTable: "GroupSettings",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Color = table.Column<string>(type: "TEXT", nullable: false),
                    GroupId = table.Column<int>(type: "INTEGER", nullable: false),
                    PermissionsId = table.Column<int>(type: "INTEGER", nullable: false),
                    ChannelId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Roles_Channels_ChannelId",
                        column: x => x.ChannelId,
                        principalTable: "Channels",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Roles_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Roles_RolePermissions_PermissionsId",
                        column: x => x.PermissionsId,
                        principalTable: "RolePermissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AccountRole",
                columns: table => new
                {
                    AccountsId = table.Column<int>(type: "INTEGER", nullable: false),
                    RolesId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountRole", x => new { x.AccountsId, x.RolesId });
                    table.ForeignKey(
                        name: "FK_AccountRole_Accounts_AccountsId",
                        column: x => x.AccountsId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccountRole_Roles_RolesId",
                        column: x => x.RolesId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Groups_OwnerId",
                table: "Groups",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Groups_SettingsId",
                table: "Groups",
                column: "SettingsId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_GroupSettingsId",
                table: "Accounts",
                column: "GroupSettingsId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountRole_RolesId",
                table: "AccountRole",
                column: "RolesId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_GroupSettingsId",
                table: "RolePermissions",
                column: "GroupSettingsId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_ChannelId",
                table: "Roles",
                column: "ChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_GroupId",
                table: "Roles",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_PermissionsId",
                table: "Roles",
                column: "PermissionsId");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_GroupSettings_GroupSettingsId",
                table: "Accounts",
                column: "GroupSettingsId",
                principalTable: "GroupSettings",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Groups_Accounts_OwnerId",
                table: "Groups",
                column: "OwnerId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Groups_GroupSettings_SettingsId",
                table: "Groups",
                column: "SettingsId",
                principalTable: "GroupSettings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_GroupSettings_GroupSettingsId",
                table: "Accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_Groups_Accounts_OwnerId",
                table: "Groups");

            migrationBuilder.DropForeignKey(
                name: "FK_Groups_GroupSettings_SettingsId",
                table: "Groups");

            migrationBuilder.DropTable(
                name: "AccountRole");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "GroupSettings");

            migrationBuilder.DropIndex(
                name: "IX_Groups_OwnerId",
                table: "Groups");

            migrationBuilder.DropIndex(
                name: "IX_Groups_SettingsId",
                table: "Groups");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_GroupSettingsId",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "SettingsId",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "IsRestricted",
                table: "Channels");

            migrationBuilder.DropColumn(
                name: "GroupSettingsId",
                table: "Accounts");
        }
    }
}
