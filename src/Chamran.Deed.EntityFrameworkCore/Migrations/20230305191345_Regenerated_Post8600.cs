using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chamran.Deed.Migrations
{
    /// <inheritdoc />
    public partial class RegeneratedPost8600 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSpecial",
                table: "Posts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PostTitle",
                table: "Posts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PostId",
                table: "Hashtags",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Hashtags_PostId",
                table: "Hashtags",
                column: "PostId");

            migrationBuilder.AddForeignKey(
                name: "FK_Hashtags_Posts_PostId",
                table: "Hashtags",
                column: "PostId",
                principalTable: "Posts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Hashtags_Posts_PostId",
                table: "Hashtags");

            migrationBuilder.DropIndex(
                name: "IX_Hashtags_PostId",
                table: "Hashtags");

            migrationBuilder.DropColumn(
                name: "IsSpecial",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "PostTitle",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "PostId",
                table: "Hashtags");
        }
    }
}
