using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chamran.Deed.Migrations
{
    /// <inheritdoc />
    public partial class Regenerated_SoftwareUpdate3333 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdatePath",
                table: "SoftwareUpdates");

            migrationBuilder.AddColumn<Guid>(
                name: "UpdateFile",
                table: "SoftwareUpdates",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdateFile",
                table: "SoftwareUpdates");

            migrationBuilder.AddColumn<string>(
                name: "UpdatePath",
                table: "SoftwareUpdates",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);
        }
    }
}
