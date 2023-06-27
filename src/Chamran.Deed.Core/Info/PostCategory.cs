using Abp.Auditing;
using Abp.Domain.Entities.Auditing;
using Chamran.Deed.People;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Domain.Entities;

namespace Chamran.Deed.Info
{
    public class PostCategory : Entity
    {

        public string PostGroupDescription { get; set; }
        public Guid? FileId { get; set; }

    }
}
