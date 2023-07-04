using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chamran.Deed.Migrations
{
    /// <inheritdoc />
    public partial class uniquevisit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Seens_PostId",
                table: "Seens");

            migrationBuilder.CreateIndex(
                name: "IX_Seens_PostId_UserId",
                table: "Seens",
                columns: new[] { "PostId", "UserId" },
                unique: true,
                filter: "[UserId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Seens_PostId_UserId",
                table: "Seens");

            migrationBuilder.CreateIndex(
                name: "IX_Seens_PostId",
                table: "Seens",
                column: "PostId");
        }
    }
}
