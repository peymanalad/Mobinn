﻿using Chamran.Deed.People;
using Chamran.Deed.Info;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;
using Abp.Domain.Entities;
using Abp.Auditing;
using Chamran.Deed.Storage;
using System.Collections.Generic;

namespace Chamran.Deed.Info
{
    [Table("Posts")]
    [Audited]
    public class Post : FullAuditedEntity
    {
        //File

        public virtual Guid? PostFile { get; set; } //File, (BinaryObjectId)
        public virtual Guid? PostFile2 { get; set; } //File, (BinaryObjectId)
        public virtual Guid? PostFile3 { get; set; } //File, (BinaryObjectId)

        public virtual string PostCaption { get; set; }

        public virtual bool IsSpecial { get; set; }
        public virtual bool IsPublished { get; set; }

        public virtual string PostTitle { get; set; }

        public virtual int? GroupMemberId { get; set; }

        [ForeignKey("GroupMemberId")]
        public GroupMember GroupMemberFk { get; set; }

        public virtual int? PostGroupId { get; set; }

        [ForeignKey("PostGroupId")]
        public PostGroup PostGroupFk { get; set; }

        [ForeignKey("PostFile")]
        public BinaryObject AppBinaryObjectFk { get; set; }

        [ForeignKey("PostFile2")]
        public BinaryObject AppBinaryObjectFk2 { get; set; }

        [ForeignKey("PostFile3")]
        public BinaryObject AppBinaryObjectFk3 { get; set; }

        public virtual string PostRefLink { get; set; }

        public virtual ICollection<PostLike> PostLikes { get; set; } // Collection of PostLikes
        
    }
}