using System;

namespace Chamran.Deed.Info;

public class GetEntriesDetail
{
    public int Id { get; set; }
    public string Caption { get; set; }
    public Guid SharedTaskId { get; set; }
    public int PostId { get; set; }
    public long IssuerId { get; set; }
    public long ReceiverId { get; set; }
    public int? ParentId { get; set; }
    public DateTime CreationTime { get; set; }
    public long? CreatorUserId { get; set; }
    public string IssuerFirstName { get; set; }
    public string IssuerLastName { get; set; }
    public Guid? IssuerProfilePicture { get; set; }
    public string ReceiverFirstName { get; set; }
    public string ReceiverLastName { get; set; }
    public Guid? ReceiverProfilePicture { get; set; }

    public bool ReturnedToParent { get; set; }
    //public string IssuerMemberPos { get; set; }
    //public string ReceiverMemberPos { get; set; }
}