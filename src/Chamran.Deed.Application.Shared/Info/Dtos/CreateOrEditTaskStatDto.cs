using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace Chamran.Deed.Info.Dtos
{
    public class CreateOrEditTaskStatDto : EntityDto<int?>
    {

        [Required]
        [StringLength(TaskStatConsts.MaxCaptionLength, MinimumLength = TaskStatConsts.MinCaptionLength)]
        public string Caption { get; set; }

        public short Status { get; set; }

        public Guid SharedTaskId { get; set; }

        public long? DoneBy { get; set; }

    }
}