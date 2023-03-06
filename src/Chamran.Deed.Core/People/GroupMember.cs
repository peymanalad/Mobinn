using Chamran.Deed.Authorization.Users;
using Chamran.Deed.People;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;
using Abp.Domain.Entities;
using Abp.Auditing;

namespace Chamran.Deed.People
{
    [Table("GroupMembers")]
    [Audited]
    public class GroupMember : Entity
    {

        [StringLength(GroupMemberConsts.MaxMemberPositionLength, MinimumLength = GroupMemberConsts.MinMemberPositionLength)]
        public virtual string MemberPosition { get; set; }

        public virtual long? UserId { get; set; }

        [ForeignKey("UserId")]
        public User UserFk { get; set; }

        public virtual int? OrganizationGroupId { get; set; }

        [ForeignKey("OrganizationGroupId")]
        public OrganizationGroup OrganizationGroupFk { get; set; }

    }
}