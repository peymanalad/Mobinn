using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chamran.Deed.Migrations
{
    /// <inheritdoc />
    public partial class SeenUserColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "Seens",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Seens_UserId",
                table: "Seens",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Seens_AbpUsers_UserId",
                table: "Seens",
                column: "UserId",
                principalTable: "AbpUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Seens_AbpUsers_UserId",
                table: "Seens");

            migrationBuilder.DropIndex(
                name: "IX_Seens_UserId",
                table: "Seens");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Seens");
        }
    }
}
