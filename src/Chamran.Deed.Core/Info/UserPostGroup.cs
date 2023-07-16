using Chamran.Deed.Authorization.Users;
using Chamran.Deed.Info;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;
using Abp.Domain.Entities;

namespace Chamran.Deed.Info
{
    [Table("UserPostGroups")]
    public class UserPostGroup : CreationAuditedEntity
    {

        public virtual long UserId { get; set; }

        [ForeignKey("UserId")]
        public User UserFk { get; set; }

        public virtual int PostGroupId { get; set; }

        [ForeignKey("PostGroupId")]
        public PostGroup PostGroupFk { get; set; }

    }
}