using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chamran.Deed.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAuditLog_Parameters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // تغییر نوع ستون Parameters به nvarchar(MAX)
            migrationBuilder.AlterColumn<string>(
                name: "Parameters",
                table: "AbpAuditLogs",
                type: "nvarchar(max)",  // تغییر به nvarchar(MAX)
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1024)",
                oldMaxLength: 1024,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // بازگردانی نوع ستون Parameters به nvarchar(1024)
            migrationBuilder.AlterColumn<string>(
                name: "Parameters",
                table: "AbpAuditLogs",
                type: "nvarchar(1024)",  // تغییر به nvarchar(1024)
                maxLength: 1024,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}