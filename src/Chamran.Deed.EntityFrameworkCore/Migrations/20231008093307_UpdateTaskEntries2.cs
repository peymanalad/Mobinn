using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chamran.Deed.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTaskEntries2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
             name: "IX_TaskEntries_IssuerId",
             table: "TaskEntries");

            migrationBuilder.CreateIndex(
                name: "IX_TaskEntries_IssuerId",
                table: "TaskEntries",
                column: "PostId",
                unique: false); // Change this to false to remove uniqueness constraint
            migrationBuilder.DropIndex(
           name: "IX_TaskEntries_ParentId",
           table: "TaskEntries");

            migrationBuilder.CreateIndex(
                name: "IX_TaskEntries_ParentId",
                table: "TaskEntries",
                column: "PostId",
                unique: false); // Change this to false to remove uniqueness constraint
            migrationBuilder.DropIndex(
           name: "IX_TaskEntries_ReceiverId",
           table: "TaskEntries");

            migrationBuilder.CreateIndex(
                name: "IX_TaskEntries_ReceiverId",
                table: "TaskEntries",
                column: "PostId",
                unique: false); // Change this to false to remove uniqueness constraint
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
            name: "IX_TaskEntries_IssuerId",
            table: "TaskEntries");

            migrationBuilder.CreateIndex(
                name: "IX_TaskEntries_IssuerId",
                table: "TaskEntries",
                column: "PostId",
                unique: false); // Change this to false to remove uniqueness constraint
            migrationBuilder.DropIndex(
           name: "IX_TaskEntries_ParentId",
           table: "TaskEntries");

            migrationBuilder.CreateIndex(
                name: "IX_TaskEntries_ParentId",
                table: "TaskEntries",
                column: "PostId",
                unique: false); // Change this to false to remove uniqueness constraint
            migrationBuilder.DropIndex(
           name: "IX_TaskEntries_ReceiverId",
           table: "TaskEntries");

            migrationBuilder.CreateIndex(
                name: "IX_TaskEntries_ReceiverId",
                table: "TaskEntries",
                column: "PostId",
                unique: false); // Change this to false to remove uniqueness constraint
        }
    }
}
