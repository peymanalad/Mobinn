using Abp.Application.Services.Dto;
using System;

namespace Chamran.Deed.Info.Dtos
{
    public class GetAllUserLocationsInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }

        public decimal? MaxUserLatFilter { get; set; }
        public decimal? MinUserLatFilter { get; set; }

        public decimal? MaxUserLongFilter { get; set; }
        public decimal? MinUserLongFilter { get; set; }

        public string UserNameFilter { get; set; }

    }
}