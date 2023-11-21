using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;
using Abp.Domain.Entities;

namespace Chamran.Deed.Info
{
    [Table("InstagramCrawlerPosts")]
    public class InstagramCrawlerPost : CreationAuditedEntity
    {

        [StringLength(InstagramCrawlerPostConsts.MaxPostCaptionLength, MinimumLength = InstagramCrawlerPostConsts.MinPostCaptionLength)]
        public virtual string PostCaption { get; set; }

        [StringLength(InstagramCrawlerPostConsts.MaxPageIdLength, MinimumLength = InstagramCrawlerPostConsts.MinPageIdLength)]
        public virtual string PageId { get; set; }

        public virtual string File1Url { get; set; }

        public virtual string File2Url { get; set; }

        public virtual string File3Url { get; set; }

        public virtual string File4Url { get; set; }

        public virtual string File5Url { get; set; }

        public virtual string File6Url { get; set; }

        public virtual string File7Url { get; set; }

        public virtual string File8Url { get; set; }

        public virtual string File9Url { get; set; }

        public virtual string File10Url { get; set; }

        public virtual DateTime? PostTime { get; set; }

        public virtual string MediaId { get; set; }

    }
}