using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chamran.Deed.Migrations
{
    /// <inheritdoc />
    public partial class AddIsPdf : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PdfFile",
                table: "Posts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Posts_PdfFile",
                table: "Posts",
                column: "PdfFile");

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_AppBinaryObjects_PdfFile",
                table: "Posts",
                column: "PdfFile",
                principalTable: "AppBinaryObjects",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Posts_AppBinaryObjects_PdfFile",
                table: "Posts");

            migrationBuilder.DropIndex(
                name: "IX_Posts_PdfFile",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "PdfFile",
                table: "Posts");
        }
    }
}
