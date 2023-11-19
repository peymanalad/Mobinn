using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chamran.Deed.Migrations
{
    /// <inheritdoc />
    public partial class GetPostForViewDDL : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
        -- Drop the procedure if it already exists
        IF OBJECT_ID('GetFilteredPosts', 'P') IS NOT NULL
            DROP PROCEDURE GetFilteredPosts;
        GO

        -- Create the updated procedure
        CREATE PROCEDURE GetFilteredPosts
            @OrganizationId INT = NULL,
            @Filter NVARCHAR(MAX) = NULL,
            @PostCaptionFilter NVARCHAR(MAX) = NULL,
            @IsSpecialFilter INT = NULL,
            @PostTitleFilter NVARCHAR(MAX) = NULL,
            @GroupMemberMemberPositionFilter NVARCHAR(MAX) = NULL,
            @PostGroupPostGroupDescriptionFilter NVARCHAR(MAX) = NULL,
            @FromDate DATETIME = NULL,
            @ToDate DATETIME = NULL,
            @OrderBy NVARCHAR(MAX) = 'Id ASC', -- Default order by Id ascending
            @MaxResultCount INT = NULL,
            @SkipCount INT = 0
        AS
        BEGIN
            DECLARE @Sql NVARCHAR(MAX)

            SET @Sql = '
            SELECT
                p.Id,
                p.PostFile,
                p.PostCaption,
                p.IsSpecial,
                p.IsPublished,
                p.PostTitle,
                p.PostRefLink,
                p.CreationTime,
                p.LastModificationTime,
                gm.MemberPosition AS GroupMemberMemberPosition,
                pg.PostGroupDescription AS PostGroupPostGroupDescription,
                pg.GroupFile AS GroupFile,
                pg.OrganizationId AS OrganizationId,
                o.OrganizationName AS OrganizationName,
                COUNT(pl.PostId) AS TotalLikes,
                COUNT(s.PostId) AS TotalVisits
            FROM
                Posts p
            INNER JOIN
                PostGroups pg ON p.PostGroupId = pg.Id
            LEFT JOIN
                GroupMembers gm ON p.GroupMemberId = gm.Id
            LEFT JOIN
                PostLikes pl ON p.Id = pl.PostId
            LEFT JOIN
                Seens s ON p.Id = s.PostId
            LEFT JOIN
                Organizations o ON pg.OrganizationId = o.Id
            WHERE
                (@OrganizationId IS NULL OR pg.OrganizationId = @OrganizationId)
                AND (@Filter IS NULL OR p.PostCaption LIKE ''%'' + @Filter + ''%'' OR p.PostTitle LIKE ''%'' + @Filter + ''%'')
                AND (@PostCaptionFilter IS NULL OR p.PostCaption LIKE ''%'' + @PostCaptionFilter + ''%'')
                AND (@IsSpecialFilter IS NULL OR (p.IsSpecial = 1 AND @IsSpecialFilter = 1) OR (p.IsSpecial = 0 AND @IsSpecialFilter = 0))
                AND (@PostTitleFilter IS NULL OR p.PostTitle LIKE ''%'' + @PostTitleFilter + ''%'')
                AND (@GroupMemberMemberPositionFilter IS NULL OR gm.MemberPosition = @GroupMemberMemberPositionFilter)
                AND (@PostGroupPostGroupDescriptionFilter IS NULL OR pg.PostGroupDescription = @PostGroupPostGroupDescriptionFilter)
                AND (@FromDate IS NULL OR p.CreationTime >= @FromDate)
                AND (@ToDate IS NULL OR p.CreationTime <= @ToDate)
            GROUP BY
                p.Id,
                p.PostFile,
                p.PostCaption,
                p.IsSpecial,
                p.IsPublished,
                p.PostTitle,
                p.PostRefLink,
                p.CreationTime,
                p.LastModificationTime,
                gm.MemberPosition,
                pg.PostGroupDescription,
                pg.GroupFile,
                pg.OrganizationId,
                o.OrganizationName'

            SET @Sql = @Sql + '
            ORDER BY ' + @OrderBy

            IF @MaxResultCount IS NOT NULL
                SET @Sql = @Sql + '
                OFFSET ' + CAST(@SkipCount AS NVARCHAR(MAX)) + ' ROWS
                FETCH NEXT ' + CAST(@MaxResultCount AS NVARCHAR(MAX)) + ' ROWS ONLY'

            EXEC sp_executesql @Sql, N'@OrganizationId INT, @Filter NVARCHAR(MAX), @PostCaptionFilter NVARCHAR(MAX), @IsSpecialFilter INT, @PostTitleFilter NVARCHAR(MAX), @GroupMemberMemberPositionFilter NVARCHAR(MAX), @PostGroupPostGroupDescriptionFilter NVARCHAR(MAX), @FromDate DATETIME, @ToDate DATETIME', @OrganizationId, @Filter, @PostCaptionFilter, @IsSpecialFilter, @PostTitleFilter, @GroupMemberMemberPositionFilter, @PostGroupPostGroupDescriptionFilter, @FromDate, @ToDate
        END;
    ");
        }


        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
