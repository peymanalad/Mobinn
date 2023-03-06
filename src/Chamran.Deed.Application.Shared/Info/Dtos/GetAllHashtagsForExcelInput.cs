using Abp.Application.Services.Dto;
using System;

namespace Chamran.Deed.Info.Dtos
{
    public class GetAllHashtagsForExcelInput
    {
        public string Filter { get; set; }

        public string HashtagTitleFilter { get; set; }

        public string PostPostTitleFilter { get; set; }

        public int? PostIdFilter { get; set; }
    }
}