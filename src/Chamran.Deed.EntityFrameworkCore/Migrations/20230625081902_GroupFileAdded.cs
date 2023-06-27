using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chamran.Deed.Migrations
{
    /// <inheritdoc />
    public partial class GroupFileAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PostGroupDescription",
                table: "Posts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PostGroupPicture",
                table: "Posts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "GroupFile",
                table: "PostGroups",
                type: "uniqueidentifier",
                nullable: true);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PostGroups_AppBinaryObjects_GroupFile",
                table: "PostGroups");

            migrationBuilder.DropIndex(
                name: "IX_PostGroups_GroupFile",
                table: "PostGroups");

            migrationBuilder.DropColumn(
                name: "PostGroupDescription",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "PostGroupPicture",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "GroupFile",
                table: "PostGroups");
        }
    }
}
