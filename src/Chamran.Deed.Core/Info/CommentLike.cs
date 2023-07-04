using Chamran.Deed.Info;
using Chamran.Deed.Authorization.Users;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;
using Abp.Domain.Entities;

namespace Chamran.Deed.Info
{
    [Table("CommentLikes")]
    public class CommentLike : Entity
    {

        public virtual DateTime LikeTime { get; set; }

        public virtual int CommentId { get; set; }

        [ForeignKey("CommentId")]
        public Comment CommentFk { get; set; }

        public virtual long UserId { get; set; }

        [ForeignKey("UserId")]
        public User UserFk { get; set; }

    }
}