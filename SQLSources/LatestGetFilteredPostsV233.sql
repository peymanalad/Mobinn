USE [DeedDb]
GO
/****** Object:  StoredProcedure [dbo].[GetFilteredPosts]    Script Date: 2024/12/04 5:39:15 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[GetFilteredPosts]
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
        p.PostSubGroupId,
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
		psg.PostSubGroupDescription AS PostGroupPostSubGroupDescription,
        o.OrganizationName AS OrganizationName,
        COUNT(pl.PostId) AS TotalLikes,
        COUNT(s.PostId) AS TotalVisits,
        u.Username AS PublisherUserName,
        u.Name AS PublisherUserFirstName,
        u.Surname AS PublisherUserLastName
    FROM
        Posts p
    INNER JOIN
        PostGroups pg ON p.PostGroupId = pg.Id
	LEFT JOIN
		PostSubGroups psg on pg.Id=psg.PostGroupId
    LEFT JOIN
        GroupMembers gm ON p.GroupMemberId = gm.Id
    LEFT JOIN
        PostLikes pl ON p.Id = pl.PostId
    LEFT JOIN
        Seens s ON p.Id = s.PostId
    LEFT JOIN
        Organizations o ON pg.OrganizationId = o.Id
    LEFT JOIN
        AbpUsers u ON p.PublisherUserId = u.Id
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
        AND p.IsDeleted <> 1
    GROUP BY
        p.Id,
        p.PostSubGroupId,
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
		psg.PostSubGroupDescription,
        o.OrganizationName,
        u.Username,
        u.Name,
        u.Surname'

    SET @Sql = @Sql + '
    ORDER BY ' + @OrderBy

    IF @MaxResultCount IS NOT NULL
        SET @Sql = @Sql + '
        OFFSET ' + CAST(@SkipCount AS NVARCHAR(MAX)) + ' ROWS
        FETCH NEXT ' + CAST(@MaxResultCount AS NVARCHAR(MAX)) + ' ROWS ONLY'

    EXEC sp_executesql @Sql, N'@OrganizationId INT, @Filter NVARCHAR(MAX), @PostCaptionFilter NVARCHAR(MAX), @IsSpecialFilter INT, @PostTitleFilter NVARCHAR(MAX), @GroupMemberMemberPositionFilter NVARCHAR(MAX), @PostGroupPostGroupDescriptionFilter NVARCHAR(MAX), @FromDate DATETIME, @ToDate DATETIME', 
        @OrganizationId, 
        @Filter, 
        @PostCaptionFilter, 
        @IsSpecialFilter, 
        @PostTitleFilter, 
        @GroupMemberMemberPositionFilter, 
        @PostGroupPostGroupDescriptionFilter, 
        @FromDate, 
        @ToDate
END;
