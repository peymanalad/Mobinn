using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace Chamran.Deed.Info.Dtos
{
    public class CreateOrEditOrganizationUserDto : EntityDto<int?>
    {

        public long UserId { get; set; }

        public int OrganizationChartId { get; set; }

        public bool IsGlobal { get; set; }

    }
    public class CreateOrEditGlobalUserDto : EntityDto<int?>
    {

        public long UserId { get; set; }

        public int OrganizationId { get; set; }


    }
}