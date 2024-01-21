using Chamran.Deed.Info;
using Chamran.Deed.Authorization.Users;
using Chamran.Deed.Authorization.Users;
using Chamran.Deed.Info;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;
using Abp.Domain.Entities;
using Abp.Auditing;

namespace Chamran.Deed.Info
{
    [Table("TaskEntries")]
    [Audited]
    public class TaskEntry : CreationAuditedEntity
    {

        [Required]
        [StringLength(TaskEntryConsts.MaxCaptionLength, MinimumLength = TaskEntryConsts.MinCaptionLength)]
        public virtual string Caption { get; set; }

        public virtual Guid SharedTaskId { get; set; }

        public virtual int PostId { get; set; }

        [ForeignKey("PostId")]
        public Post PostFk { get; set; }

        public virtual long IssuerId { get; set; }

        [ForeignKey("IssuerId")]
        public User IssuerFk { get; set; }

        public virtual long ReceiverId { get; set; }

        [ForeignKey("ReceiverId")]
        public User ReceiverFk { get; set; }

        public virtual int? ParentId { get; set; }

        [ForeignKey("ParentId")]
        public TaskEntry ParentFk { get; set; }

        public bool IsPrivate { get; set; }
        public bool ReturnedToParent { get; set; }
        public bool IsSeen { get; set; }

    }
}