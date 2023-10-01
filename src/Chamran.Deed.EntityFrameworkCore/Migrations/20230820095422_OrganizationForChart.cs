using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chamran.Deed.Migrations
{
    /// <inheritdoc />
    public partial class OrganizationForChart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                table: "OrganizationCharts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationCharts_OrganizationId",
                table: "OrganizationCharts",
                column: "OrganizationId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrganizationCharts_Organizations_OrganizationId",
                table: "OrganizationCharts",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrganizationCharts_Organizations_OrganizationId",
                table: "OrganizationCharts");

            migrationBuilder.DropIndex(
                name: "IX_OrganizationCharts_OrganizationId",
                table: "OrganizationCharts");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "OrganizationCharts");
        }
    }
}
