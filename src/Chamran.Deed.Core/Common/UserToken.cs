using Chamran.Deed.Authorization.Users;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;
using Abp.Domain.Entities;

namespace Chamran.Deed.Common
{
    [Table("UserTokens")]
    public class UserToken : CreationAuditedEntity
    {

        [Required]
        [StringLength(UserTokenConsts.MaxTokenLength, MinimumLength = UserTokenConsts.MinTokenLength)]
        public virtual string Token { get; set; }

        public virtual long? UserId { get; set; }

        [ForeignKey("UserId")]
        public User UserFk { get; set; }

    }
}