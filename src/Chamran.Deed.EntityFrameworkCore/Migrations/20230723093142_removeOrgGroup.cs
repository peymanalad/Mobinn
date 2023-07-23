using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chamran.Deed.Migrations
{
    /// <inheritdoc />
    public partial class removeOrgGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupMembers_OrganizationGroups_OrganizationGroupId",
                table: "GroupMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_PostGroups_OrganizationGroups_OrganizationGroupId",
                table: "PostGroups");

            migrationBuilder.DropTable(
                name: "OrganizationGroups");

            migrationBuilder.DropIndex(
                name: "IX_GroupMember_UserId_OrganizationGroupId",
                table: "GroupMembers");

            migrationBuilder.RenameColumn(
                name: "OrganizationGroupId",
                table: "PostGroups",
                newName: "OrganizationId");

            migrationBuilder.RenameIndex(
                name: "IX_PostGroups_OrganizationGroupId",
                table: "PostGroups",
                newName: "IX_PostGroups_OrganizationId");

            migrationBuilder.RenameColumn(
                name: "OrganizationGroupId",
                table: "GroupMembers",
                newName: "OrganizationId");

            migrationBuilder.RenameIndex(
                name: "IX_GroupMembers_OrganizationGroupId",
                table: "GroupMembers",
                newName: "IX_GroupMembers_OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupMembers_UserId",
                table: "GroupMembers",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupMembers_Organizations_OrganizationId",
                table: "GroupMembers",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PostGroups_Organizations_OrganizationId",
                table: "PostGroups",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupMembers_Organizations_OrganizationId",
                table: "GroupMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_PostGroups_Organizations_OrganizationId",
                table: "PostGroups");

            migrationBuilder.DropIndex(
                name: "IX_GroupMembers_UserId",
                table: "GroupMembers");

            migrationBuilder.RenameColumn(
                name: "OrganizationId",
                table: "PostGroups",
                newName: "OrganizationGroupId");

            migrationBuilder.RenameIndex(
                name: "IX_PostGroups_OrganizationId",
                table: "PostGroups",
                newName: "IX_PostGroups_OrganizationGroupId");

            migrationBuilder.RenameColumn(
                name: "OrganizationId",
                table: "GroupMembers",
                newName: "OrganizationGroupId");

            migrationBuilder.RenameIndex(
                name: "IX_GroupMembers_OrganizationId",
                table: "GroupMembers",
                newName: "IX_GroupMembers_OrganizationGroupId");

            migrationBuilder.CreateTable(
                name: "OrganizationGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrganizationId = table.Column<int>(type: "int", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GroupName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganizationGroups_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_GroupMember_UserId_OrganizationGroupId",
                table: "GroupMembers",
                columns: new[] { "UserId", "OrganizationGroupId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationGroups_OrganizationId",
                table: "OrganizationGroups",
                column: "OrganizationId");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupMembers_OrganizationGroups_OrganizationGroupId",
                table: "GroupMembers",
                column: "OrganizationGroupId",
                principalTable: "OrganizationGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PostGroups_OrganizationGroups_OrganizationGroupId",
                table: "PostGroups",
                column: "OrganizationGroupId",
                principalTable: "OrganizationGroups",
                principalColumn: "Id");
        }
    }
}
