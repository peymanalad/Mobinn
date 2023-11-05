using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chamran.Deed.Migrations
{
    /// <inheritdoc />
    public partial class foreignkeyforfiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Posts_PostFile10",
                table: "Posts",
                column: "PostFile10");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_PostFile4",
                table: "Posts",
                column: "PostFile4");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_PostFile5",
                table: "Posts",
                column: "PostFile5");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_PostFile6",
                table: "Posts",
                column: "PostFile6");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_PostFile7",
                table: "Posts",
                column: "PostFile7");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_PostFile8",
                table: "Posts",
                column: "PostFile8");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_PostFile9",
                table: "Posts",
                column: "PostFile9");

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_AppBinaryObjects_PostFile10",
                table: "Posts",
                column: "PostFile10",
                principalTable: "AppBinaryObjects",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_AppBinaryObjects_PostFile4",
                table: "Posts",
                column: "PostFile4",
                principalTable: "AppBinaryObjects",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_AppBinaryObjects_PostFile5",
                table: "Posts",
                column: "PostFile5",
                principalTable: "AppBinaryObjects",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_AppBinaryObjects_PostFile6",
                table: "Posts",
                column: "PostFile6",
                principalTable: "AppBinaryObjects",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_AppBinaryObjects_PostFile7",
                table: "Posts",
                column: "PostFile7",
                principalTable: "AppBinaryObjects",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_AppBinaryObjects_PostFile8",
                table: "Posts",
                column: "PostFile8",
                principalTable: "AppBinaryObjects",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_AppBinaryObjects_PostFile9",
                table: "Posts",
                column: "PostFile9",
                principalTable: "AppBinaryObjects",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Posts_AppBinaryObjects_PostFile10",
                table: "Posts");

            migrationBuilder.DropForeignKey(
                name: "FK_Posts_AppBinaryObjects_PostFile4",
                table: "Posts");

            migrationBuilder.DropForeignKey(
                name: "FK_Posts_AppBinaryObjects_PostFile5",
                table: "Posts");

            migrationBuilder.DropForeignKey(
                name: "FK_Posts_AppBinaryObjects_PostFile6",
                table: "Posts");

            migrationBuilder.DropForeignKey(
                name: "FK_Posts_AppBinaryObjects_PostFile7",
                table: "Posts");

            migrationBuilder.DropForeignKey(
                name: "FK_Posts_AppBinaryObjects_PostFile8",
                table: "Posts");

            migrationBuilder.DropForeignKey(
                name: "FK_Posts_AppBinaryObjects_PostFile9",
                table: "Posts");

            migrationBuilder.DropIndex(
                name: "IX_Posts_PostFile10",
                table: "Posts");

            migrationBuilder.DropIndex(
                name: "IX_Posts_PostFile4",
                table: "Posts");

            migrationBuilder.DropIndex(
                name: "IX_Posts_PostFile5",
                table: "Posts");

            migrationBuilder.DropIndex(
                name: "IX_Posts_PostFile6",
                table: "Posts");

            migrationBuilder.DropIndex(
                name: "IX_Posts_PostFile7",
                table: "Posts");

            migrationBuilder.DropIndex(
                name: "IX_Posts_PostFile8",
                table: "Posts");

            migrationBuilder.DropIndex(
                name: "IX_Posts_PostFile9",
                table: "Posts");
        }
    }
}
