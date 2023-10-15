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

        public virtual bool IsGovernmental { get; set; }

        [StringLength(OrganizationConsts.MaxNationalIdLength, MinimumLength = OrganizationConsts.MinNationalIdLength)]
        public virtual string NationalId { get; set; }

        [StringLength(OrganizationConsts.MaxOrganizationLocationLength, MinimumLength = OrganizationConsts.MinOrganizationLocationLength)]
        public virtual string OrganizationLocation { get; set; }

        [StringLength(OrganizationConsts.MaxOrganizationPhoneLength, MinimumLength = OrganizationConsts.MinOrganizationPhoneLength)]
        public virtual string OrganizationPhone { get; set; }

        [StringLength(OrganizationConsts.MaxOrganizationContactPersonLength, MinimumLength = OrganizationConsts.MinOrganizationContactPersonLength)]
        public virtual string OrganizationContactPerson { get; set; }

        [StringLength(OrganizationConsts.MaxCommentLength, MinimumLength = OrganizationConsts.MinCommentLength)]
        public virtual string Comment { get; set; }
        //File

        public virtual Guid? OrganizationLogo { get; set; } //File, (BinaryObjectId)

    }
}