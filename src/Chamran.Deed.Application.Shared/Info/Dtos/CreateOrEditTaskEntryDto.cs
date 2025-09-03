using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace Chamran.Deed.Info.Dtos
{
    public class CreateOrEditTaskEntryDto : EntityDto<int?>
    {

        [Required]
        [StringLength(TaskEntryConsts.MaxCaptionLength, MinimumLength = TaskEntryConsts.MinCaptionLength)]
        public string Caption { get; set; }

        public Guid SharedTaskId { get; set; }

        public int? PostId { get; set; }
        public long? IssuerId { get; set; }
        public long? ReceiverId { get; set; }
        public int? ParentId { get; set; }
        public bool IsPrivate{ get; set; }
        public bool ReturnedToParent  { get; set; }
        public bool IsSeen { get; set; }

    }
}