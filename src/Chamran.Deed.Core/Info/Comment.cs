using Chamran.Deed.Info;
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
    [Table("Comments")]
    [Audited]
    public class Comment : FullAuditedEntity
    {

        [Required]
        [StringLength(CommentConsts.MaxCommentCaptionLength, MinimumLength = CommentConsts.MinCommentCaptionLength)]
        public virtual string CommentCaption { get; set; }

        public virtual DateTime InsertDate { get; set; }

        public virtual int PostId { get; set; }

        [ForeignKey("PostId")]
        public Post PostFk { get; set; }

        public virtual long UserId { get; set; }

        [ForeignKey("UserId")]
        public User UserFk { get; set; }

        public virtual int? CommentId { get; set; }

        [ForeignKey("CommentId")]
        public Comment CommentFk { get; set; }

    }
}