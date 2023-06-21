using System;

namespace Chamran.Deed.Info.Dtos
{
    public class GetPostsForViewDto
    {
        public int Id { get; set; }
        public Guid? PostFile { get; set; }
        public string PostCaption { get; set; }
        public DateTime PostCreation { get; set; }
        public int GroupMemberId { get; set; }
        public string MemberPosition { get; set; }
        public string MemberUserName { get; set; }
        public string MemberFullName { get; set; }
        public bool IsSpecial { get; set; }
        public string PostTitle { get; set; }
        public string Attachment1 { get; set; }
        public Guid? PostFile2 { get; set; }
        public string Attachment2 { get; set; }
        public Guid? PostFile3 { get; set; }
        public string Attachment3 { get; set; }

    }
}