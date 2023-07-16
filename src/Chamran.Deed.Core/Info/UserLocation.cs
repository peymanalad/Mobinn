using Chamran.Deed.Authorization.Users;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;
using Abp.Domain.Entities;

namespace Chamran.Deed.Info
{
    [Table("UserLocations")]
    public class UserLocation : CreationAuditedEntity
    {

        public virtual decimal UserLat { get; set; }

        public virtual decimal UserLong { get; set; }

        public virtual long UserId { get; set; }

        [ForeignKey("UserId")]
        public User UserFk { get; set; }

    }
}