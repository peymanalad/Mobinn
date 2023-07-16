using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace Chamran.Deed.Info.Dtos
{
    public class CreateOrEditUserLocationDto : EntityDto<int?>
    {

        public decimal UserLat { get; set; }

        public decimal UserLong { get; set; }

        public long UserId { get; set; }

    }
}