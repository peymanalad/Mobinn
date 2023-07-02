using Chamran.Deed.Info;
using Chamran.Deed.Authorization.Users;
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

        public virtual int? ParentId { get; set; }

        public virtual DateTime CommentDate { get; set; }

        public virtual int PostId { get; set; }

        [ForeignKey("PostId")]
        public Post PostFk { get; set; }

        public virtual long UserId { get; set; }

        [ForeignKey("UserId")]
        public User UserFk { get; set; }

    }
}