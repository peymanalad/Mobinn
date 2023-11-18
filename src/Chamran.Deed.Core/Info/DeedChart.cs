using Chamran.Deed.People;
using Chamran.Deed.Info;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;
using Abp.Domain.Entities;
using System.Collections.Generic;

namespace Chamran.Deed.Info
{
    [Table("DeedCharts")]
    public class DeedChart : CreationAuditedEntity
    {

        [Required]
        [StringLength(DeedChartConsts.MaxCaptionLength, MinimumLength = DeedChartConsts.MinCaptionLength)]
        public virtual string Caption { get; set; }

        [StringLength(DeedChartConsts.MaxLeafPathLength, MinimumLength = DeedChartConsts.MinLeafPathLength)]
        public virtual string LeafPath { get; set; }  
        [StringLength(DeedChartConsts.MaxLeafPathLength, MinimumLength = DeedChartConsts.MinLeafPathLength)]
        public virtual string LeafCationPath { get; set; }

        public virtual int? OrganizationId { get; set; }

        [ForeignKey("OrganizationId")]
        public Organization OrganizationFk { get; set; }

        public virtual int? ParentId { get; set; }

        [ForeignKey("ParentId")]
        public DeedChart ParentFk { get; set; }

        // Navigation property for children
        public virtual ICollection<DeedChart> Children { get; set; } = new List<DeedChart>();

        public void GenerateLeafPath()
        {
            if (ParentFk == null)
            {
                LeafPath = Id + "\\";
                LeafCationPath = Caption + "\\";

            }
            else
            {
                var parentPath = ParentFk.LeafPath;
                LeafPath = $"{parentPath}{Id}\\";
                var parentCaptionPath = ParentFk.LeafCationPath;
                LeafCationPath = $"{parentCaptionPath}{Caption}\\";

            }

            foreach (var child in Children)
            {
                child.GenerateLeafPath();
            }
        }

    }
}