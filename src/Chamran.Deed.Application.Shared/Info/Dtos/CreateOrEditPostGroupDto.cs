using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace Chamran.Deed.Info.Dtos
{
    public class CreateOrEditPostGroupDto : EntityDto<int?>
    {

        public string PostGroupDescription { get; set; }

        public int? OrganizationGroupId { get; set; }

    }
}