using System;

public class PostExploreDto
{
    public int Id { get; set; }
    public string ThumbnailPath { get; set; }
    public string PreviewPath { get; set; }
    public bool IsVideo { get; set; }
    public string PostCaption { get; set; }
    public DateTime CreationTime { get; set; }

    public int GroupMemberId { get; set; }
    public string PostTitle { get; set; }
    public bool IsSpecial { get; set; }
    public bool IsPublished { get; set; }
    public int? PostGroupId { get; set; }
    public string PostRefLink { get; set; }

    public string MemberFullName { get; set; }
    public string MemberUserName { get; set; }
    public string MemberPosition { get; set; }

    public Guid? GroupFile { get; set; }
    public string GroupDescription { get; set; }
    public string PostSubGroupDescription { get; set; }
}