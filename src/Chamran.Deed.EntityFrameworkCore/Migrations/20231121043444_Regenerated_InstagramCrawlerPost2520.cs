using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chamran.Deed.Migrations
{
    /// <inheritdoc />
    public partial class Regenerated_InstagramCrawlerPost2520 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InstagramCrawlerPosts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PostCaption = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    PageId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    File1Url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    File2Url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    File3Url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    File4Url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    File5Url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    File6Url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    File7Url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    File8Url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    File9Url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    File10Url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PostTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MediaId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstagramCrawlerPosts", x => x.Id);
                });

           
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InstagramCrawlerPosts");

           
        }
    }
}
