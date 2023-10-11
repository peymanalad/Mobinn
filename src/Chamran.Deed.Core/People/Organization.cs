using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;
using Abp.Domain.Entities;

namespace Chamran.Deed.People
{
    [Table("Organizations")]
    public class Organization : FullAuditedEntity
    {

        [Required]
        [StringLength(OrganizationConsts.MaxOrganizationNameLength, MinimumLength = OrganizationConsts.MinOrganizationNameLength)]
        public virtual string OrganizationName { get; set; }

    }
}