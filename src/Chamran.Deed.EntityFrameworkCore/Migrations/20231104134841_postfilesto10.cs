using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chamran.Deed.Migrations
{
    /// <inheritdoc />
    public partial class postfilesto10 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PostFile10",
                table: "Posts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PostFile4",
                table: "Posts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PostFile5",
                table: "Posts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PostFile6",
                table: "Posts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PostFile7",
                table: "Posts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PostFile8",
                table: "Posts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PostFile9",
                table: "Posts",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PostFile10",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "PostFile4",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "PostFile5",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "PostFile6",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "PostFile7",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "PostFile8",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "PostFile9",
                table: "Posts");

        }
    }
}
