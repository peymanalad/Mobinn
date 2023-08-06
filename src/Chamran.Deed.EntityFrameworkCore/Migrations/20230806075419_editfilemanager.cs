using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chamran.Deed.Migrations
{
    /// <inheritdoc />
    public partial class editfilemanager : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SourceGuid",
                table: "AppBinaryObjects",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SourceId",
                table: "AppBinaryObjects",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SourceType",
                table: "AppBinaryObjects",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SourceGuid",
                table: "AppBinaryObjects");

            migrationBuilder.DropColumn(
                name: "SourceId",
                table: "AppBinaryObjects");

            migrationBuilder.DropColumn(
                name: "SourceType",
                table: "AppBinaryObjects");
        }
    }
}
