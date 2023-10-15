using Chamran.Deed.Authorization.Users;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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

        public virtual int MemberPos { get; set; }

        [Required]
        public virtual long? UserId { get; set; }

        [ForeignKey("UserId")]
        public User UserFk { get; set; }

        [Required]
        public virtual int? OrganizationId { get; set; }

        [ForeignKey("OrganizationId")]
        public Organization OrganizationFk { get; set; }
    }
}