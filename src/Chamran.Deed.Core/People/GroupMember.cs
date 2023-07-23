using Chamran.Deed.Authorization.Users;
using Chamran.Deed.People;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;
using Abp.Domain.Entities;
using Abp.Auditing;
using Microsoft.EntityFrameworkCore;

namespace Chamran.Deed.People
{
    [Table("GroupMembers")]
    [Audited]
    [Index(nameof(UserId), nameof(OrganizationGroupId), IsDescending = new[] { false, false }, Name = "IX_GroupMember_UserId_OrganizationGroupId",IsUnique = true)]
    public class GroupMember : Entity
    {

        [StringLength(GroupMemberConsts.MaxMemberPositionLength, MinimumLength = GroupMemberConsts.MinMemberPositionLength)]
        public virtual string MemberPosition { get; set; }

        public virtual int MemberPos { get; set; }

        [Required]
        public virtual long? UserId { get; set; }

        [ForeignKey("UserId")]
        public User UserFk { get; set; }

        [Required]
        public virtual int? OrganizationGroupId { get; set; }

        [ForeignKey("OrganizationGroupId")]
        public OrganizationGroup OrganizationGroupFk { get; set; }
    }
}