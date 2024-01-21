using System;
using Abp.Application.Services.Dto;

namespace Chamran.Deed.Info.Dtos
{
    public class TaskEntryDto : EntityDto
    {
        public string Caption { get; set; }

        public Guid SharedTaskId { get; set; }

        public int PostId { get; set; }

        public long IssuerId { get; set; }

        public long ReceiverId { get; set; }

        public int? ParentId { get; set; }

        public bool IsSeen { get; set; }

    }
}