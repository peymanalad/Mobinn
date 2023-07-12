using Abp.Application.Services.Dto;
using System;

namespace Chamran.Deed.Common.Dtos
{
    public class GetAllUserTokensInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }

        public string TokenFilter { get; set; }

        public string UserNameFilter { get; set; }

    }
}