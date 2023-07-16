using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace Chamran.Deed.Info.Dtos
{
    public class GetPostGroupForEditOutput
    {
        public CreateOrEditPostGroupDto PostGroup { get; set; }

        public string OrganizationGroupGroupName { get; set; }

        public string GroupFileFileName { get; set; }

    }
}