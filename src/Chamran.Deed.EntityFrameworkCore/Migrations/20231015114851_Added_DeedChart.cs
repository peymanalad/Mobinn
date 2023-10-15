using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chamran.Deed.Migrations
{
    /// <inheritdoc />
    public partial class Added_DeedChart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
         

            migrationBuilder.CreateTable(
                name: "DeedCharts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Caption = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LeafPath = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    OrganizationId = table.Column<int>(type: "int", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeedCharts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeedCharts_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id");
                });

    

          

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            

            migrationBuilder.DropTable(
                name: "DeedCharts");

         
           
        }
    }
}
