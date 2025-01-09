using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;

namespace Chamran.Deed.Info.Dtos
{
    public class GetAllPostsInput : PagedAndSortedResultRequestDto
    {
        public int? OrganizationId { get; set; }
        public string Filter { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string PostCaptionFilter { get; set; }
        public int? IsSpecialFilter { get; set; }
        public string PostTitleFilter { get; set; }
        public string GroupMemberMemberPositionFilter { get; set; }
        public string PostGroupPostGroupDescriptionFilter { get; set; }
        public string PostGroupPostSubGroupDescriptionFilter { get; set; } // New field
    }

}