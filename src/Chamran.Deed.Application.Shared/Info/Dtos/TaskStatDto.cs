using System;
using Abp.Application.Services.Dto;

namespace Chamran.Deed.Info.Dtos
{
    public class TaskStatDto : EntityDto
    {
        public string Caption { get; set; }

        public short Status { get; set; }

        public Guid SharedTaskId { get; set; }

        public long? DoneBy { get; set; }

    }
}