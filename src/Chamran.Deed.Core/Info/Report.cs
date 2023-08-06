using Chamran.Deed.People;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;
using Abp.Domain.Entities;
using Abp.Auditing;

namespace Chamran.Deed.Info
{
    [Table("Reports")]
    [Audited]
    public class Report : FullAuditedEntity
    {

        [Required]
        [StringLength(ReportConsts.MaxReportDescriptionLength, MinimumLength = ReportConsts.MinReportDescriptionLength)]
        public virtual string ReportDescription { get; set; }

        public virtual bool IsDashboard { get; set; }

        [Required]
        public virtual string ReportContent { get; set; }

        public virtual int? OrganizationId { get; set; }

        [ForeignKey("OrganizationId")]
        public Organization OrganizationFk { get; set; }

        public bool IsSuperUser { get; set; }

    }
}