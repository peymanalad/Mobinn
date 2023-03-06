using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chamran.Deed.Migrations
{
    /// <inheritdoc />
    public partial class RegeneratedPost5960 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PostGroupId",
                table: "Posts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Posts_PostGroupId",
                table: "Posts",
                column: "PostGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_PostGroups_PostGroupId",
                table: "Posts",
                column: "PostGroupId",
                principalTable: "PostGroups",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Posts_PostGroups_PostGroupId",
                table: "Posts");

            migrationBuilder.DropIndex(
                name: "IX_Posts_PostGroupId",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "PostGroupId",
                table: "Posts");
        }
    }
}
