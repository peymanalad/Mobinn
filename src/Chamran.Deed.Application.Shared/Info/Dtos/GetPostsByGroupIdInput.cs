using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chamran.Deed.Info.Dtos
{
    public class GetPostsByGroupIdInput: PagedAndSortedResultRequestDto
    {
        public int PostGroupId { get; set; }
        public int  OrganizationId { get; set; }
        
    }
}
