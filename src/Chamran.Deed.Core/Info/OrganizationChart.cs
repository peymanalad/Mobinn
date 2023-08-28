using Chamran.Deed.Info;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;
using Abp.Domain.Entities;
using System.Collections.Generic;
using Chamran.Deed.People;

namespace Chamran.Deed.Info
{
    [Table("OrganizationCharts")]
    public class OrganizationChart : CreationAuditedEntity
    {

        [Required]
        [StringLength(OrganizationChartConsts.MaxCaptionLength, MinimumLength = OrganizationChartConsts.MinCaptionLength)]
        public virtual string Caption { get; set; }

        [StringLength(OrganizationChartConsts.MaxLeafPathLength, MinimumLength = OrganizationChartConsts.MinLeafPathLength)]
        public virtual string LeafPath { get; set; }

        public virtual int? ParentId { get; set; }

        public int? OrganizationId { get; set; }
        
        [ForeignKey("OrganizationId")]
        public Organization OrganizationFk { get; set; }

        [ForeignKey("ParentId")]
        public OrganizationChart ParentFk { get; set; }
        
        // Navigation property for children
        public virtual ICollection<OrganizationChart> Children { get; set; } = new List<OrganizationChart>();

        public void GenerateLeafPath()
        {
            if (ParentFk == null)
            {
                LeafPath = Id + "\\";
            }
            else
            {
                var parentPath = ParentFk.LeafPath;
                LeafPath = $"{parentPath}{Id}\\";
            }

            foreach (var child in Children)
            {
                child.GenerateLeafPath();
            }
        }

    }
}