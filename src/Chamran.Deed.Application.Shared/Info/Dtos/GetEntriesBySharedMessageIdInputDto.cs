using System;
using Abp.Application.Services.Dto;

namespace Chamran.Deed.Info.Dtos
{
    public class GetEntriesBySharedMessageIdInputDto : PagedAndSortedResultRequestDto
    {
        public Guid SharedTaskId { get; set; }
        public string CaptionFilter{ get; set; }



    }
}