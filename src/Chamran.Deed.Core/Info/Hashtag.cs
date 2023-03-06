using Chamran.Deed.Info;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;
using Abp.Domain.Entities;

namespace Chamran.Deed.Info
{
    [Table("Hashtags")]
    public class Hashtag : Entity
    {

        public virtual string HashtagTitle { get; set; }

        public virtual int? PostId { get; set; }

        [ForeignKey("PostId")]
        public Post PostFk { get; set; }

    }
}