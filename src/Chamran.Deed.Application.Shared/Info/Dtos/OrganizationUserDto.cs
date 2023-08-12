using System;
using Abp.Application.Services.Dto;

namespace Chamran.Deed.Info.Dtos
{
    public class OrganizationUserDto : EntityDto
    {

        public long UserId { get; set; }

        public int OrganizationChartId { get; set; }

    }
}