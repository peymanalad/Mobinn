 -- Delete CommentLikes
    DELETE FROM CommentLikes
    WHERE CommentId IN (
        SELECT c.Id
        FROM Comments c
        INNER JOIN Posts p ON c.PostId = p.Id
        INNER JOIN GroupMembers gm ON p.GroupMemberId = gm.Id
        INNER JOIN Organizations o ON gm.OrganizationId = o.Id
        WHERE o.IsDeleted = 1
    );

    -- Delete Comments
    DELETE FROM Comments
    WHERE PostId IN (
        SELECT p.Id
        FROM Posts p
        INNER JOIN GroupMembers gm ON p.GroupMemberId = gm.Id
        INNER JOIN Organizations o ON gm.OrganizationId = o.Id
        WHERE o.IsDeleted = 1
    );

    -- Delete TaskEntries
    DELETE FROM TaskEntries
    WHERE PostId IN (
        SELECT p.Id
        FROM Posts p
        INNER JOIN GroupMembers gm ON p.GroupMemberId = gm.Id
        INNER JOIN Organizations o ON gm.OrganizationId = o.Id
        WHERE o.IsDeleted = 1
    );

    -- Delete Posts
    DELETE FROM Posts
    WHERE GroupMemberId IN (
        SELECT gm.Id
        FROM GroupMembers gm
        INNER JOIN Organizations o ON gm.OrganizationId = o.Id
        WHERE o.IsDeleted = 1
    );

    -- Delete GroupMembers
    DELETE FROM GroupMembers
    WHERE OrganizationId IN (
        SELECT o.Id
        FROM Organizations o
        WHERE o.IsDeleted = 1
    );

    -- Delete Reports
    DELETE FROM Reports
    WHERE OrganizationId IN (
        SELECT o.Id
        FROM Organizations o
        WHERE o.IsDeleted = 1
    );

    -- Delete PostGroups
    DELETE FROM PostGroups
    WHERE OrganizationId IN (
        SELECT o.Id
        FROM Organizations o
        WHERE o.IsDeleted = 1
    );

    -- Delete OrganizationCharts
    DELETE FROM OrganizationCharts
    WHERE OrganizationId IN (
        SELECT o.Id
        FROM Organizations o
        WHERE o.IsDeleted = 1
    );

    -- Finally, delete Organizations
    DELETE FROM Organizations
    WHERE IsDeleted = 1;




	Delete from dbo.CommentLikes
	Where CommentId in (
	 SELECT c.Id
        FROM Comments c
        INNER JOIN Posts p ON c.PostId = p.Id
		Where p.IsDeleted=1)

	Delete From dbo.PostLikes 
	where PostId in (select id from dbo.Posts where isDeleted=1)

	delete from dbo.Comments
	where PostId in (select id from dbo.Posts where isDeleted=1) 

	Delete From dbo.Posts
	Where IsDeleted=1

	delete from dbo.PostGroups
	where isDeleted=1
