using Chamran.Deed.People;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;
using Abp.Auditing;
using Chamran.Deed.Storage;
using System.Collections.Generic;
using Chamran.Deed.Info.Dtos;

namespace Chamran.Deed.Info
{
    [Table("Posts")]
    [Audited]
    public class Post : FullAuditedEntity<int>
    {
        public long? PublisherUserId { get; set; }
        public long? CreatorUserId { get; set; }
        public DateTime? DatePublished { get; set; }
        public PostStatus CurrentPostStatus { get; set; }
        public string PostComment { get; set; }

        public virtual Guid PostKey { get; set; }
        //File

        public virtual Guid? PostFile { get; set; } //File, (BinaryObjectId)
        public virtual Guid? PostFile2 { get; set; } //File, (BinaryObjectId)
        public virtual Guid? PostFile3 { get; set; } //File, (BinaryObjectId)
        public virtual Guid? PostFile4 { get; set; } //File, (BinaryObjectId)
        public virtual Guid? PostFile5 { get; set; } //File, (BinaryObjectId)
        public virtual Guid? PostFile6 { get; set; } //File, (BinaryObjectId)
        public virtual Guid? PostFile7 { get; set; } //File, (BinaryObjectId)
        public virtual Guid? PostFile8 { get; set; } //File, (BinaryObjectId)
        public virtual Guid? PostFile9 { get; set; } //File, (BinaryObjectId)
        public virtual Guid? PostFile10 { get; set; } //File, (BinaryObjectId)

        public virtual string PostCaption { get; set; }

        public virtual bool IsSpecial { get; set; }
        public virtual bool IsPublished { get; set; }


        public virtual string PostTitle { get; set; }

        public virtual int? GroupMemberId { get; set; }

        [ForeignKey("GroupMemberId")]
        public GroupMember GroupMemberFk { get; set; }

        public virtual int? PostGroupId { get; set; }
        public virtual int? PostSubGroupId { get; set; }

        [ForeignKey("PostGroupId")]
        public PostGroup PostGroupFk { get; set; }

        [ForeignKey("PostSubGroupId")]
        public PostSubGroup PostSubGroupFk { get; set; }


        [ForeignKey("PostFile")]
        public BinaryObject AppBinaryObjectFk { get; set; }

        [ForeignKey("PostFile2")]
        public BinaryObject AppBinaryObjectFk2 { get; set; }

        [ForeignKey("PostFile3")]
        public BinaryObject AppBinaryObjectFk3 { get; set; }

        [ForeignKey("PostFile4")]
        public BinaryObject AppBinaryObjectFk4 { get; set; }


        [ForeignKey("PostFile5")]
        public BinaryObject AppBinaryObjectFk5 { get; set; }


        [ForeignKey("PostFile6")]
        public BinaryObject AppBinaryObjectFk6 { get; set; }


        [ForeignKey("PostFile7")]
        public BinaryObject AppBinaryObjectFk7 { get; set; }


        [ForeignKey("PostFile8")]
        public BinaryObject AppBinaryObjectFk8 { get; set; }


        [ForeignKey("PostFile9")]
        public BinaryObject AppBinaryObjectFk9 { get; set; }


        [ForeignKey("PostFile10")]
        public BinaryObject AppBinaryObjectFk10 { get; set; }

        public virtual string PostRefLink { get; set; }

        public virtual ICollection<PostLike> PostLikes { get; set; } // Collection of PostLikes
        public virtual ICollection<PostEditHistory> EditHistories { get; set; } = new List<PostEditHistory>();


    }
}