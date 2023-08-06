using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp;
using Abp.Domain.Entities;
using Chamran.Deed.Common;

namespace Chamran.Deed.Storage
{
    [Table("AppBinaryObjects")]
    public class BinaryObject : Entity<Guid>, IMayHaveTenant
    {
        public virtual int? TenantId { get; set; }

        public virtual string Description { get; set; }

        [Required]
        [MaxLength(BinaryObjectConsts.BytesMaxSize)]
        public virtual byte[] Bytes { get; set; }

        public int? SourceType { get; set; }

        public int? SourceId { get; set; }

        public Guid? SourceGuid { get; set; }

        public BinaryObject()
        {
            Id = SequentialGuidGenerator.Instance.Create();
        }

        public BinaryObject(int? tenantId, byte[] bytes, BinarySourceType sourceType, string description = null)
            : this()
        {
            TenantId = tenantId;
            Bytes = bytes;
            Description = description;
            SourceType = (int?)sourceType;
        }

    }
}
