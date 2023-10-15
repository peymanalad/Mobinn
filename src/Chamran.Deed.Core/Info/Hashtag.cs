using System.ComponentModel.DataAnnotations.Schema;
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