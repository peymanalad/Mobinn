using Chamran.Deed.Info;
using Chamran.Deed.Authorization.Users;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;
using Abp.Domain.Entities;

namespace Chamran.Deed.Info
{
    [Table("PostLikes")]
    public class PostLike : Entity
    {

        public virtual DateTime LikeTime { get; set; }

        public virtual int PostId { get; set; }

        [ForeignKey("PostId")]
        public Post PostFk { get; set; }

        public virtual long UserId { get; set; }

        [ForeignKey("UserId")]
        public User UserFk { get; set; }

    }
}