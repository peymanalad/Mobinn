using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chamran.Deed.Migrations
{
    /// <inheritdoc />
    public partial class leafPathNotReq : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.AddColumn<string>(
            //    name: "Comment",
            //    table: "Organizations",
            //    type: "nvarchar(255)",
            //    maxLength: 255,
            //    nullable: true);

            //migrationBuilder.AddColumn<bool>(
            //    name: "IsGovernmental",
            //    table: "Organizations",
            //    type: "bit",
            //    nullable: false,
            //    defaultValue: false);

            //migrationBuilder.AddColumn<string>(
            //    name: "NationalId",
            //    table: "Organizations",
            //    type: "nvarchar(14)",
            //    maxLength: 14,
            //    nullable: true);

            //migrationBuilder.AddColumn<string>(
            //    name: "OrganizationContactPerson",
            //    table: "Organizations",
            //    type: "nvarchar(128)",
            //    maxLength: 128,
            //    nullable: true);

            //migrationBuilder.AddColumn<string>(
            //    name: "OrganizationLocation",
            //    table: "Organizations",
            //    type: "nvarchar(255)",
            //    maxLength: 255,
            //    nullable: true);

            //migrationBuilder.AddColumn<Guid>(
            //    name: "OrganizationLogo",
            //    table: "Organizations",
            //    type: "uniqueidentifier",
            //    nullable: true);

            //migrationBuilder.AddColumn<string>(
            //    name: "OrganizationPhone",
            //    table: "Organizations",
            //    type: "nvarchar(255)",
            //    maxLength: 255,
            //    nullable: true);

            //migrationBuilder.AddColumn<int>(
            //    name: "OrganizationId",
            //    table: "OrganizationCharts",
            //    type: "int",
            //    nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LeafPath",
                table: "DeedCharts",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(400)",
                oldMaxLength: 400);

            //migrationBuilder.CreateIndex(
            //    name: "IX_OrganizationCharts_OrganizationId",
            //    table: "OrganizationCharts",
            //    column: "OrganizationId");

            //migrationBuilder.AddForeignKey(
            //    name: "FK_OrganizationCharts_Organizations_OrganizationId",
            //    table: "OrganizationCharts",
            //    column: "OrganizationId",
            //    principalTable: "Organizations",
            //    principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropForeignKey(
            //    name: "FK_OrganizationCharts_Organizations_OrganizationId",
            //    table: "OrganizationCharts");

            //migrationBuilder.DropIndex(
            //    name: "IX_OrganizationCharts_OrganizationId",
            //    table: "OrganizationCharts");

            //migrationBuilder.DropColumn(
            //    name: "Comment",
            //    table: "Organizations");

            //migrationBuilder.DropColumn(
            //    name: "IsGovernmental",
            //    table: "Organizations");

            //migrationBuilder.DropColumn(
            //    name: "NationalId",
            //    table: "Organizations");

            //migrationBuilder.DropColumn(
            //    name: "OrganizationContactPerson",
            //    table: "Organizations");

            //migrationBuilder.DropColumn(
            //    name: "OrganizationLocation",
            //    table: "Organizations");

            //migrationBuilder.DropColumn(
            //    name: "OrganizationLogo",
            //    table: "Organizations");

            //migrationBuilder.DropColumn(
            //    name: "OrganizationPhone",
            //    table: "Organizations");

            //migrationBuilder.DropColumn(
            //    name: "OrganizationId",
            //    table: "OrganizationCharts");

            migrationBuilder.AlterColumn<string>(
                name: "LeafPath",
                table: "DeedCharts",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(400)",
                oldMaxLength: 400,
                oldNullable: true);
        }
    }
}
