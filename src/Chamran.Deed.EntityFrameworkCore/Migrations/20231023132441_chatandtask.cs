using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chamran.Deed.Migrations
{
    /// <inheritdoc />
    public partial class chatandtask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPublished",
                table: "Posts",
                type: "bit",
                nullable: false,
                defaultValue: false);


            migrationBuilder.AddColumn<string>(
                name: "ForwardedFromName",
                table: "AppChatMessages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ReplyMessageId",
                table: "AppChatMessages",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            
            migrationBuilder.DropColumn(
                name: "IsPublished",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "ForwardedFromName",
                table: "AppChatMessages");

            migrationBuilder.DropColumn(
                name: "ReplyMessageId",
                table: "AppChatMessages");
        }
    }
}
