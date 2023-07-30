using Abp.Application.Services.Dto;
using System;

namespace Chamran.Deed.Info.Dtos
{
    public class GetAllPostsForExcelInput
    {
        public string Filter { get; set; }

        public string PostCaptionFilter { get; set; }

        public int? IsSpecialFilter { get; set; }

        public string PostTitleFilter { get; set; }

        public string GroupMemberMemberPositionFilter { get; set; }

        public string PostGroupPostGroupDescriptionFilter { get; set; }

    }
}