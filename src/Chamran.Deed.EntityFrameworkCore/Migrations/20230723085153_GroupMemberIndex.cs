using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chamran.Deed.Migrations
{
    /// <inheritdoc />
    public partial class GroupMemberIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupMembers_AbpUsers_UserId",
                table: "GroupMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupMembers_OrganizationGroups_OrganizationGroupId",
                table: "GroupMembers");

            migrationBuilder.DropIndex(
                name: "IX_GroupMembers_UserId",
                table: "GroupMembers");

            migrationBuilder.AlterColumn<long>(
                name: "UserId",
                table: "GroupMembers",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "OrganizationGroupId",
                table: "GroupMembers",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GroupMember_UserId_OrganizationGroupId",
                table: "GroupMembers",
                columns: new[] { "UserId", "OrganizationGroupId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupMembers_AbpUsers_UserId",
                table: "GroupMembers",
                column: "UserId",
                principalTable: "AbpUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupMembers_OrganizationGroups_OrganizationGroupId",
                table: "GroupMembers",
                column: "OrganizationGroupId",
                principalTable: "OrganizationGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupMembers_AbpUsers_UserId",
                table: "GroupMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupMembers_OrganizationGroups_OrganizationGroupId",
                table: "GroupMembers");

            migrationBuilder.DropIndex(
                name: "IX_GroupMember_UserId_OrganizationGroupId",
                table: "GroupMembers");

            migrationBuilder.AlterColumn<long>(
                name: "UserId",
                table: "GroupMembers",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<int>(
                name: "OrganizationGroupId",
                table: "GroupMembers",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_GroupMembers_UserId",
                table: "GroupMembers",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupMembers_AbpUsers_UserId",
                table: "GroupMembers",
                column: "UserId",
                principalTable: "AbpUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupMembers_OrganizationGroups_OrganizationGroupId",
                table: "GroupMembers",
                column: "OrganizationGroupId",
                principalTable: "OrganizationGroups",
                principalColumn: "Id");
        }
    }
}
