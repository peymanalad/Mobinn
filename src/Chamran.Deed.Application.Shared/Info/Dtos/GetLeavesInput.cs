﻿using Abp.Application.Services.Dto;

namespace Chamran.Deed.Info.Dtos
{
    public class GetLeavesInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }
    }
}