using System;

namespace Chamran.Deed.Info.Dtos
{

    public class SendPostNotificationDto 
    {
        public Guid? PostFile { get; set; }
        public Guid? PdfFile { get; set; }
        public string PostFileFileName { get; set; }
        public string PdfFileFileName { get; set; }

        public string PostCaption { get; set; }

        public bool IsSpecial { get; set; }
        public bool IsPublished { get; set; }

        public string PostTitle { get; set; }

        public int? GroupMemberId { get; set; }

        public int? PostGroupId { get; set; }

        public string PostRefLink { get; set; }

        public DateTime CreationTime { get; set; }
        public DateTime? LastModificationTime { get; set; }

        public Guid? GroupFile { get; set; }
        public string GroupDescription { get; set; }
    }
}