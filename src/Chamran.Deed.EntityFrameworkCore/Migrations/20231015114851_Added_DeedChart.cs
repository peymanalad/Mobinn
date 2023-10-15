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
            migrationBuilder.DropForeignKey(
                name: "FK_TaskEntries_AbpUsers_IssuerId",
                table: "TaskEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskEntries_AbpUsers_ReceiverId",
                table: "TaskEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskEntries_Posts_PostId",
                table: "TaskEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskEntries_TaskEntries_ParentId",
                table: "TaskEntries");

            migrationBuilder.DropIndex(
                name: "IX_TaskEntries_IssuerId",
                table: "TaskEntries");

            migrationBuilder.DropIndex(
                name: "IX_TaskEntries_ParentId",
                table: "TaskEntries");

            migrationBuilder.DropIndex(
                name: "IX_TaskEntries_PostId",
                table: "TaskEntries");

            migrationBuilder.DropIndex(
                name: "IX_TaskEntries_ReceiverId",
                table: "TaskEntries");

            migrationBuilder.DropIndex(
                name: "IX_GroupMembers_UserId",
                table: "GroupMembers");

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

            migrationBuilder.CreateTable(
                name: "EntriesDigest",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Caption = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SharedTaskId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PostId = table.Column<int>(type: "int", nullable: false),
                    IssuerId = table.Column<long>(type: "bigint", nullable: false),
                    ReceiverId = table.Column<long>(type: "bigint", nullable: false),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true),
                    IssuerFirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IssuerLastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IssuerProfilePicture = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReceiverFirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReceiverLastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReceiverProfilePicture = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IssuerMemberPos = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReceiverMemberPos = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PostFile = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PostCaption = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PostGroupMemberId = table.Column<int>(type: "int", nullable: true),
                    PostCreationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PostCreatorUserId = table.Column<long>(type: "bigint", nullable: true),
                    PostLastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PostLastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    PostGroupId = table.Column<int>(type: "int", nullable: true),
                    IsSpecial = table.Column<bool>(type: "bit", nullable: true),
                    PostTitle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PostFile2 = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PostFile3 = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PostRefLink = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaskEntries_IssuerId",
                table: "TaskEntries",
                column: "IssuerId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskEntries_ParentId",
                table: "TaskEntries",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskEntries_PostId",
                table: "TaskEntries",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskEntries_ReceiverId",
                table: "TaskEntries",
                column: "ReceiverId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupMembers_UserId_OrganizationId",
                table: "GroupMembers",
                columns: new[] { "UserId", "OrganizationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeedCharts_OrganizationId",
                table: "DeedCharts",
                column: "OrganizationId");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskEntries_AbpUsers_IssuerId",
                table: "TaskEntries",
                column: "IssuerId",
                principalTable: "AbpUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskEntries_AbpUsers_ReceiverId",
                table: "TaskEntries",
                column: "ReceiverId",
                principalTable: "AbpUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskEntries_Posts_PostId",
                table: "TaskEntries",
                column: "PostId",
                principalTable: "Posts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskEntries_TaskEntries_ParentId",
                table: "TaskEntries",
                column: "ParentId",
                principalTable: "TaskEntries",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskEntries_AbpUsers_IssuerId",
                table: "TaskEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskEntries_AbpUsers_ReceiverId",
                table: "TaskEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskEntries_Posts_PostId",
                table: "TaskEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskEntries_TaskEntries_ParentId",
                table: "TaskEntries");

            migrationBuilder.DropTable(
                name: "DeedCharts");

            migrationBuilder.DropTable(
                name: "EntriesDigest");

            migrationBuilder.DropIndex(
                name: "IX_TaskEntries_IssuerId",
                table: "TaskEntries");

            migrationBuilder.DropIndex(
                name: "IX_TaskEntries_ParentId",
                table: "TaskEntries");

            migrationBuilder.DropIndex(
                name: "IX_TaskEntries_PostId",
                table: "TaskEntries");

            migrationBuilder.DropIndex(
                name: "IX_TaskEntries_ReceiverId",
                table: "TaskEntries");

            migrationBuilder.DropIndex(
                name: "IX_GroupMembers_UserId_OrganizationId",
                table: "GroupMembers");

            migrationBuilder.CreateIndex(
                name: "IX_TaskEntries_IssuerId",
                table: "TaskEntries",
                column: "IssuerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaskEntries_ParentId",
                table: "TaskEntries",
                column: "ParentId",
                unique: true,
                filter: "[ParentId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TaskEntries_PostId",
                table: "TaskEntries",
                column: "PostId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaskEntries_ReceiverId",
                table: "TaskEntries",
                column: "ReceiverId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GroupMembers_UserId",
                table: "GroupMembers",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskEntries_AbpUsers_IssuerId",
                table: "TaskEntries",
                column: "IssuerId",
                principalTable: "AbpUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskEntries_AbpUsers_ReceiverId",
                table: "TaskEntries",
                column: "ReceiverId",
                principalTable: "AbpUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskEntries_Posts_PostId",
                table: "TaskEntries",
                column: "PostId",
                principalTable: "Posts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskEntries_TaskEntries_ParentId",
                table: "TaskEntries",
                column: "ParentId",
                principalTable: "TaskEntries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
