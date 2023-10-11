using Microsoft.EntityFrameworkCore.Migrations;

namespace Chamran.Deed.Migrations
{
    public partial class UpdateTaskEntries : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
              name: "IX_TaskEntries_PostId",
              table: "TaskEntries");

            migrationBuilder.CreateIndex(
                name: "IX_TaskEntries_PostId",
                table: "TaskEntries",
                column: "PostId",
                unique: false); // Change this to false to remove uniqueness constraint
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TaskEntries_PostId",
                table: "TaskEntries");
            migrationBuilder.CreateIndex(
              name: "IX_TaskEntries_PostId",
              table: "TaskEntries",
              column: "PostId",
              unique: false); // Change this to false to remove uniqueness constraint
        }
    }
}
