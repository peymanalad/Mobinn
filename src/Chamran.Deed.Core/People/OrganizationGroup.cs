using Chamran.Deed.People;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;
using Abp.Domain.Entities;
using Abp.Auditing;

namespace Chamran.Deed.People
{
    [Table("OrganizationGroups")]
    [Audited]
    public class OrganizationGroup : FullAuditedEntity
    {

        [Required]
        [StringLength(OrganizationGroupConsts.MaxGroupNameLength, MinimumLength = OrganizationGroupConsts.MinGroupNameLength)]
        public virtual string GroupName { get; set; }

        public virtual int? OrganizationId { get; set; }

        [ForeignKey("OrganizationId")]
        public Organization OrganizationFk { get; set; }

    }
}