using Chamran.Deed.People;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;
using Abp.Domain.Entities;
using Abp.Auditing;

namespace Chamran.Deed.Info
{
    [Table("PostSubGroups")]
    [Audited]
    public class PostSubGroup : FullAuditedEntity
    {

        public virtual string PostSubGroupDescription { get; set; }

        public virtual int Ordering { get; set; }
        //File

        public virtual Guid? GroupFile { get; set; } //File, (BinaryObjectId)

        public virtual int? PostGroupId { get; set; }

        [ForeignKey("PostGroupId")]
        public PostGroup PostGroupFk { get; set; }

    }
}