using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace Chamran.Deed.Info.Dtos
{
    public class CreateOrEditOrganizationUserDto : EntityDto<int?>
    {

        public long UserId { get; set; }

        public int OrganizationChartId { get; set; }

    }
}