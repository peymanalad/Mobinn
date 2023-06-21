using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chamran.Deed.Migrations
{
    /// <inheritdoc />
    public partial class SecondThirdPost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PostFile2",
                table: "Posts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PostFile3",
                table: "Posts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Posts_PostFile",
                table: "Posts",
                column: "PostFile");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_PostFile2",
                table: "Posts",
                column: "PostFile2");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_PostFile3",
                table: "Posts",
                column: "PostFile3");

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_AppBinaryObjects_PostFile",
                table: "Posts",
                column: "PostFile",
                principalTable: "AppBinaryObjects",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_AppBinaryObjects_PostFile2",
                table: "Posts",
                column: "PostFile2",
                principalTable: "AppBinaryObjects",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_AppBinaryObjects_PostFile3",
                table: "Posts",
                column: "PostFile3",
                principalTable: "AppBinaryObjects",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Posts_AppBinaryObjects_PostFile",
                table: "Posts");

            migrationBuilder.DropForeignKey(
                name: "FK_Posts_AppBinaryObjects_PostFile2",
                table: "Posts");

            migrationBuilder.DropForeignKey(
                name: "FK_Posts_AppBinaryObjects_PostFile3",
                table: "Posts");

            migrationBuilder.DropIndex(
                name: "IX_Posts_PostFile",
                table: "Posts");

            migrationBuilder.DropIndex(
                name: "IX_Posts_PostFile2",
                table: "Posts");

            migrationBuilder.DropIndex(
                name: "IX_Posts_PostFile3",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "PostFile2",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "PostFile3",
                table: "Posts");
        }
    }
}
