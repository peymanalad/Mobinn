using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;
using Abp.Domain.Entities;

namespace Chamran.Deed.Common
{
    [Table("SoftwareUpdates")]
    public class SoftwareUpdate : Entity
    {

        [Required]
        [StringLength(SoftwareUpdateConsts.MaxSoftwareVersionLength, MinimumLength = SoftwareUpdateConsts.MinSoftwareVersionLength)]
        public virtual string SoftwareVersion { get; set; }

        public virtual bool ForceUpdate { get; set; }

        [StringLength(SoftwareUpdateConsts.MaxWhatsNewLength, MinimumLength = SoftwareUpdateConsts.MinWhatsNewLength)]
        public virtual string WhatsNew { get; set; }

        [StringLength(SoftwareUpdateConsts.MaxPlatformLength, MinimumLength = SoftwareUpdateConsts.MinPlatformLength)]
        public virtual string Platform { get; set; }

        public virtual int BuildNo { get; set; }
        //File

        public virtual Guid? UpdateFile { get; set; } //File, (BinaryObjectId)

    }
}