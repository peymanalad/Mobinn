using System;

namespace Chamran.Deed.Authorization.Users.Dto;

public class BriefSeenPostsDto
{
    public DateTime SeenTime { get; set; }
    public int PostId { get; set; }
    public string PostTitle { get; set; }
    public Guid? PostFile { get; set; }
    public DateTime PostTime { get; set; }
}


public class BriefLikedPostsDto
{
    public int PostId { get; set; }
    public string PostTitle { get; set; }
    public Guid? PostFile { get; set; }
    public DateTime PostTime { get; set; }
}
public class BriefCommentsDto
{
    public int PostId { get; set; }
    public string PostTitle { get; set; }
    public Guid? PostFile { get; set; }
    public DateTime PostTime { get; set; }
    public int CommentId { get; set; }
    public string CommentCaption { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime InsertDate { get; set; }
}


public class BriefCreatedPostsDto
{
    public int PostId { get; set; }
    public string PostTitle { get; set; }
    public Guid? PostFile { get; set; }
    public DateTime PostTime { get; set; }
}

public class LoginInfosDto
{
    public DateTime CreationTime { get; set; }
    public string BrowserInfo { get; set; }
    public string ClientIpAddress { get; set; }
}