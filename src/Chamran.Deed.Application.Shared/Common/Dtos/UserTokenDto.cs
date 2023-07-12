using System;
using Abp.Application.Services.Dto;

namespace Chamran.Deed.Common.Dtos
{
    public class UserTokenDto : EntityDto
    {
        public string Token { get; set; }

        public long? UserId { get; set; }

    }
}