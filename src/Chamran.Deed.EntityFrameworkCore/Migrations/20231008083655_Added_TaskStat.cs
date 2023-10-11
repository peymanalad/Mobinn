using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chamran.Deed.Migrations
{
    /// <inheritdoc />
    public partial class Added_TaskStat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TaskStats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Caption = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    Status = table.Column<short>(type: "smallint", nullable: false),
                    SharedTaskId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DoneBy = table.Column<long>(type: "bigint", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskStats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskStats_AbpUsers_DoneBy",
                        column: x => x.DoneBy,
                        principalTable: "AbpUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaskStats_DoneBy",
                table: "TaskStats",
                column: "DoneBy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaskStats");
        }
    }
}
