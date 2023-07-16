using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace Chamran.Deed.Info.Dtos
{
    public class CreateOrEditUserPostGroupDto : EntityDto<int?>
    {

        public long UserId { get; set; }

        public int PostGroupId { get; set; }

    }
}