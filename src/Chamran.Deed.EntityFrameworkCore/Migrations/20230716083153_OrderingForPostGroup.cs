using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chamran.Deed.Migrations
{
    /// <inheritdoc />
    public partial class OrderingForPostGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PostGroups_AppBinaryObjects_GroupFile",
                table: "PostGroups");

            migrationBuilder.DropIndex(
                name: "IX_PostGroups_GroupFile",
                table: "PostGroups");

            migrationBuilder.AlterColumn<string>(
                name: "Token",
                table: "UserTokens",
                type: "nvarchar(1024)",
                maxLength: 1024,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AddColumn<int>(
                name: "Ordering",
                table: "PostGroups",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "DeviceToken",
                table: "FCMQueues",
                type: "nvarchar(1024)",
                maxLength: 1024,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Ordering",
                table: "PostGroups");

            migrationBuilder.AlterColumn<string>(
                name: "Token",
                table: "UserTokens",
                type: "nvarchar(1024)",
                maxLength: 1024,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1024)",
                oldMaxLength: 1024);

            migrationBuilder.AlterColumn<string>(
                name: "DeviceToken",
                table: "FCMQueues",
                type: "nvarchar(1024)",
                maxLength: 1024,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1024)",
                oldMaxLength: 1024);

            migrationBuilder.CreateIndex(
                name: "IX_PostGroups_GroupFile",
                table: "PostGroups",
                column: "GroupFile");

            migrationBuilder.AddForeignKey(
                name: "FK_PostGroups_AppBinaryObjects_GroupFile",
                table: "PostGroups",
                column: "GroupFile",
                principalTable: "AppBinaryObjects",
                principalColumn: "Id");
        }
    }
}
