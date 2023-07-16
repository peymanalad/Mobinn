using System;
using Abp.Application.Services.Dto;

namespace Chamran.Deed.Info.Dtos
{
    public class PostGroupDto : EntityDto
    {
        public string PostGroupDescription { get; set; }

        public int Ordering { get; set; }

        public Guid? GroupFile { get; set; }

        public string GroupFileFileName { get; set; }

        public int? OrganizationGroupId { get; set; }

    }
}