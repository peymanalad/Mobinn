using Chamran.Deed.People;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;
using Abp.Domain.Entities;
using Abp.Auditing;

namespace Chamran.Deed.Info
{
    [Table("PostGroups")]
    [Audited]
    public class PostGroup : FullAuditedEntity
    {

        public virtual string PostGroupDescription { get; set; }

        public virtual int? OrganizationGroupId { get; set; }

        [ForeignKey("OrganizationGroupId")]
        public OrganizationGroup OrganizationGroupFk { get; set; }

    }
}