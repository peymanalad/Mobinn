using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chamran.Deed.Migrations
{
    /// <inheritdoc />
    public partial class Regenerated_DeedChart2957 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ParentId",
                table: "DeedCharts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeedCharts_ParentId",
                table: "DeedCharts",
                column: "ParentId");

            migrationBuilder.AddForeignKey(
                name: "FK_DeedCharts_DeedCharts_ParentId",
                table: "DeedCharts",
                column: "ParentId",
                principalTable: "DeedCharts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeedCharts_DeedCharts_ParentId",
                table: "DeedCharts");

            migrationBuilder.DropIndex(
                name: "IX_DeedCharts_ParentId",
                table: "DeedCharts");

            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "DeedCharts");
        }
    }
}
