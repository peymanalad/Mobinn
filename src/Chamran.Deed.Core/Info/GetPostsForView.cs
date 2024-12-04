using System;
using System.Collections.Generic;

namespace Chamran.Deed.Info;

public class GetPostsForView
{
    public int Id { get; set; }
    public Guid? PostFile { get; set; }
    public string PostCaption { get; set; }
    public bool IsSpecial { get; set; }
    public bool IsPublished { get; set; }
    public string PostTitle { get; set; }
    public string PostRefLink { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime? LastModificationTime { get; set; }

    public string GroupMemberMemberPosition { get; set; }
    public string PostGroupPostGroupDescription { get; set; }
    public string PostGroupPostSubGroupDescription { get; set; }
    public Guid? GroupFile { get; set; }
    public int OrganizationId { get; set; }
    public string OrganizationName { get; set; }

    public int TotalLikes { get; set; }
    public int TotalVisits { get; set; }
    public int? PostSubGroupId { get; set; }
    public string PublisherUserName { get; set; }
    public string PublisherUserFirstName { get; set; }
    public string PublisherUserLastName { get; set; }
}