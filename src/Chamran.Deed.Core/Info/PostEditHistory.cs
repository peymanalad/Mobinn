using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Auditing;
using Abp.Domain.Entities.Auditing;

namespace Chamran.Deed.Info
{
    [Table("PostEditHistories")]
    [Audited]
    public class PostEditHistory : FullAuditedEntity
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        [ForeignKey("PostId")]
        public Post Post { get; set; }

        public string EditorName { get; set; }
        public DateTime EditTime { get; set; }
        public string Changes { get; set; }
    }
}
