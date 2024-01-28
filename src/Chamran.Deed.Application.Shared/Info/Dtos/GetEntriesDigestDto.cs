using System;

namespace Chamran.Deed.Info.Dtos
{
    public class GetEntriesDigestDto
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
        //public string IssuerMemberPos { get; set; }
        //public string ReceiverMemberPos { get; set; }
        public Guid? PostFile { get; set; }
        public string PostCaption { get; set; }
        public int? PostGroupMemberId { get; set; }
        public DateTime? PostCreationTime { get; set; }
        public long? PostCreatorUserId { get; set; }
        public DateTime? PostLastModificationTime { get; set; }
        public long? PostLastModifierUserId { get; set; }
        public int? PostGroupId { get; set; }
        public bool? IsSpecial { get; set; }
        public bool IsPublished { get; set; }
        public string PostTitle { get; set; }
        public Guid? PostFile2 { get; set; }
        public Guid? PostFile3 { get; set; }
        public Guid? PostFile4 { get; set; }
        public Guid? PostFile5 { get; set; }
        public Guid? PostFile6 { get; set; }
        public Guid? PostFile7 { get; set; }
        public Guid? PostFile8 { get; set; }
        public Guid? PostFile9 { get; set; }
        public Guid? PostFile10 { get; set; }
        public string PostRefLink { get; set; }
        public string PostGroupDescription { get; set; }
        public Guid? GroupFile{ get; set; }
        public bool IsPrivate { get; set; }
        public int WorkFlowCount { get; set; }
        public bool IsSeen { get; set; }

    } 
    
    public class GetEntriesDetailDto
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
        //public string IssuerMemberPos { get; set; }
        //public string ReceiverMemberPos { get; set; }
        public bool IsPrivate { get; set; }
        public bool ReturnedToParent { get; set; }
    }
}