using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chamran.Deed.Info
{
    public class GetEntriesDigest
    {
        public int Id { get; set; }
        public string Caption { get; set; }
        public int SharedTaskId { get; set; }
        public int? PostId { get; set; }
        public int IssuerId { get; set; }
        public int ReceiverId { get; set; }
        public int? ParentId { get; set; }
        public DateTime CreationTime { get; set; }
        public long? CreatorUserId { get; set; }
        public string IssuerFirstName { get; set; }
        public string IssuerLastName { get; set; }
        public int? IssuerProfilePicture { get; set; }
        public string ReceiverFirstName { get; set; }
        public string ReceiverLastName { get; set; }
        public int? ReceiverProfilePicture { get; set; }
        public string IssuerMemberPos { get; set; }
        public string ReceiverMemberPos { get; set; }
        public string PostFile { get; set; }
        public string PostCaption { get; set; }
        public int? PostGroupMemberId { get; set; }
        public DateTime? PostCreationTime { get; set; }
        public long? PostCreatorUserId { get; set; }
        public DateTime? PostLastModificationTime { get; set; }
        public long? PostLastModifierUserId { get; set; }
        public int? PostGroupId { get; set; }
        public bool? IsSpecial { get; set; }
        public string PostTitle { get; set; }
        public string PostFile2 { get; set; }
        public string PostFile3 { get; set; }
        public string PostRefLink { get; set; }
    }
}
