using System;
using Abp.Application.Services.Dto;

namespace Chamran.Deed.Info.Dtos
{
    public class UserLocationDto : EntityDto
    {
        public decimal UserLat { get; set; }

        public decimal UserLong { get; set; }

        public long UserId { get; set; }

    }
}