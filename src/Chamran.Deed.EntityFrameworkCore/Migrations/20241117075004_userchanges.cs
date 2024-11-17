using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chamran.Deed.Migrations
{
    /// <inheritdoc />
    public partial class userchanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CurrentPostStatus",
                table: "Posts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "DatePublished",
                table: "Posts",
                type: "datetime2",
                nullable: true
                );

            migrationBuilder.AddColumn<string>(
                name: "PostComment",
                table: "Posts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PostSubGroupId",
                table: "Posts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "PublisherUserId",
                table: "Posts",
                type: "bigint",
                nullable: true
                );

            migrationBuilder.AddColumn<int>(
                name: "UserType",
                table: "AbpUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            //migrationBuilder.AddColumn<string>(
            //    name: "TargetNotifiers",
            //    table: "AbpNotificationSubscriptions",
            //    type: "nvarchar(1024)",
            //    maxLength: 1024,
            //    nullable: true);

            migrationBuilder.CreateTable(
                name: "PostSubGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PostSubGroupDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ordering = table.Column<int>(type: "int", nullable: false),
                    GroupFile = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PostGroupId = table.Column<int>(type: "int", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostSubGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PostSubGroups_PostGroups_PostGroupId",
                        column: x => x.PostGroupId,
                        principalTable: "PostGroups",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Posts_PostSubGroupId",
                table: "Posts",
                column: "PostSubGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_PostSubGroups_PostGroupId",
                table: "PostSubGroups",
                column: "PostGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_PostSubGroups_PostSubGroupId",
                table: "Posts",
                column: "PostSubGroupId",
                principalTable: "PostSubGroups",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Posts_PostSubGroups_PostSubGroupId",
                table: "Posts");

            migrationBuilder.DropTable(
                name: "PostSubGroups");

            migrationBuilder.DropIndex(
                name: "IX_Posts_PostSubGroupId",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "CurrentPostStatus",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "DatePublished",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "PostComment",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "PostSubGroupId",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "PublisherUserId",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "UserType",
                table: "AbpUsers");

            //migrationBuilder.DropColumn(
            //    name: "TargetNotifiers",
            //    table: "AbpNotificationSubscriptions");
        }
    }
}
