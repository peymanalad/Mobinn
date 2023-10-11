using Chamran.Deed.Authorization.Users;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;
using Abp.Domain.Entities;
using Abp.Auditing;

namespace Chamran.Deed.Info
{
    [Table("TaskStats")]
    [Audited]
    public class TaskStat : CreationAuditedEntity
    {

        [Required]
        [StringLength(TaskStatConsts.MaxCaptionLength, MinimumLength = TaskStatConsts.MinCaptionLength)]
        public virtual string Caption { get; set; }

        public virtual short Status { get; set; }

        public virtual Guid SharedTaskId { get; set; }

        public virtual long? DoneBy { get; set; }

        [ForeignKey("DoneBy")]
        public User DoneByFk { get; set; }

    }
}