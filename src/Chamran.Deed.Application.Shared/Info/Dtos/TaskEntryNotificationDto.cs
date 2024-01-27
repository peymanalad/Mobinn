using System;
using System.ComponentModel.DataAnnotations;
using Abp.Application.Services.Dto;

namespace Chamran.Deed.Info.Dtos
{

    public class TaskEntryNotificationDto : EntityDto<int?>
    {

        [Required]
        [StringLength(TaskEntryConsts.MaxCaptionLength, MinimumLength = TaskEntryConsts.MinCaptionLength)]
        public string Caption { get; set; }
        public Guid SharedTaskId { get; set; }
        public int PostId { get; set; }
        public long IssuerId { get; set; }
        public long ReceiverId { get; set; }
        public int? ParentId { get; set; }
        public bool IsPrivate { get; set; }
        public bool ReturnedToParent { get; set; }
        public bool? IsSeen { get; set; }
        public string IssuerFirstName { get; set; }
        public string IssuerLastName { get; set; }
        public Guid? IssuerProfilePicture { get; set; }
        public string ReceiverFirstName { get; set; }
        public string ReceiverLastName { get; set; }
        public Guid? ReceiverProfilePicture { get; set; }
        public Guid? PostFile { get; set; }
        public string PostCaption { get; set; }
        public int? PostGroupMemberId { get; set; }
        public DateTime? PostCreationTime { get; set; }
        public int? PostCreatorUserId { get; set; }
        public DateTime? PostLastModificationTime { get; set; }
        public int? PostLastModifierUserId { get; set; }
        public int? PostGroupId { get; set; }
        public bool? IsSpecial { get; set; }
        public bool? IsPublished { get; set; }
        public string PostTitle { get; set; }
        public Guid? PostFile2{ get; set; }
        public Guid? PostFile3{ get; set; }
        public Guid? PostFile4{ get; set; }
        public Guid? PostFile5{ get; set; }
        public Guid? PostFile6{ get; set; }
        public Guid? PostFile7{ get; set; }
        public Guid? PostFile8 { get; set; }
        public Guid? PostFile9 { get; set; }
        public Guid? PostFile10 { get; set; }
        public string PostRefLink { get; set; }
        public string PosGroupDescription { get; set; }
        public Guid GroupFile { get; set; }
        public int? WorkFlowCount { get; set; }
    }
}