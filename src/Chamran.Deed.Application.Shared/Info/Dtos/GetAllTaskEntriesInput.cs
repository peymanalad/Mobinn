using Abp.Application.Services.Dto;
using System;

namespace Chamran.Deed.Info.Dtos
{
    public class GetAllTaskEntriesInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }

        public string CaptionFilter { get; set; }

        public Guid? SharedTaskIdFilter { get; set; }

        public string PostPostTitleFilter { get; set; }

        public string UserNameFilter { get; set; }

        public string UserName2Filter { get; set; }

        public string TaskEntryCaptionFilter { get; set; }

    }
}