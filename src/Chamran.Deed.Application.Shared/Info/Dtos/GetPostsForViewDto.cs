using System;

namespace Chamran.Deed.Info.Dtos
{
    public class GetPostsForViewDto
    {
        public int Id { get; set; }
        public Guid? PostFile { get; set; }
        public Guid? PostFile2 { get; set; }
        public Guid? PostFile3 { get; set; }
        public Guid? PostFile4 { get; set; }
        public Guid? PostFile5 { get; set; }
        public Guid? PostFile6 { get; set; }
        public Guid? PostFile7 { get; set; }
        public Guid? PostFile8 { get; set; }
        public Guid? PostFile9 { get; set; }
        public Guid? PostFile10 { get; set; }
        public Guid? PdfFile { get; set; }
        public string ThumbnailPath { get; set; }
        public string PreviewPath { get; set; }
        //public string VideoPath { get; set; }
        public bool IsPdf { get; set; }
        public bool IsSlide { get; set; }
        public bool IsVideo { get; set; }

        public string PostCaption { get; set; }
        public int GroupMemberId { get; set; }
        public string MemberPosition { get; set; }
        public string MemberUserName { get; set; }
        public string MemberFullName { get; set; }
        public bool IsSpecial { get; set; }
        public bool IsPublished { get; set; }
        public string PostTitle { get; set; }
        public string Attachment1 { get; set; }
        public string Attachment2 { get; set; }
        public string Attachment3 { get; set; }
        public string Attachment4 { get; set; }
        public string Attachment5 { get; set; }
        public string Attachment6 { get; set; }
        public string Attachment7 { get; set; }
        public string Attachment8 { get; set; }
        public string Attachment9 { get; set; }
        public string Attachment10 { get; set; }
        public int? PostGroupId { get; set; }
        public Guid? GroupFile { get; set; }
        public string GroupDescription { get; set; }
        public string PostSubGroupDescription { get; set; }
        public string PostRefLink { get; set; }
        public DateTime CreationTime { get; set; }

    }
}