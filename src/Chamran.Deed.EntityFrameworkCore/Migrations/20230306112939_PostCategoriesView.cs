using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chamran.Deed.Migrations
{
    /// <inheritdoc />
    public partial class PostCategoriesView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
WITH ranked_messages AS (
select pg.Id,p.PostCaption,pg.PostGroupDescription,p.IsSpecial,p.CreationTime,og.GroupName,o.OrganizationName,abo.[Description] as Attachment1,abo.Id as FileId,ROW_NUMBER() OVER (PARTITION BY p.PostGroupId ORDER BY p.CreationTime DESC) AS rn
from dbo.Posts p
join dbo.PostGroups pg on p.PostGroupId=pg.Id
join dbo.OrganizationGroups og on pg.OrganizationGroupId=og.Id
join dbo.Organizations o on og.OrganizationId=o.Id
join dbo.AppBinaryObjects abo on abo.Id=p.PostFile
where pg.OrganizationGroupId in (select OrganizationGroupId from GroupMembers
where UserId=2)
)
SELECT * FROM ranked_messages WHERE rn = 1
UNION ALL 
Select * from (select top(1) pg.Id,p.PostCaption,N'اخبار برگزیده' as PostGroupDescription,p.IsSpecial,p.CreationTime,og.GroupName,o.OrganizationName,abo.[Description] as Attachment1,abo.Id as FileId,ROW_NUMBER() OVER (PARTITION BY p.PostGroupId ORDER BY p.CreationTime DESC) AS rn
from dbo.Posts p
join dbo.PostGroups pg on p.PostGroupId=pg.Id
join dbo.OrganizationGroups og on pg.OrganizationGroupId=og.Id
join dbo.Organizations o on og.OrganizationId=o.Id
join dbo.AppBinaryObjects abo on abo.Id=p.PostFile
where pg.OrganizationGroupId in (select OrganizationGroupId from GroupMembers
where UserId=2) And p.IsSpecial=1 
order by p.CreationTime Desc) tbl
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DROP VIEW [dbo].[vwPostCategories]
");
        }
    }
}
