using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace Chamran.Deed.Common.Dtos
{
    public class CreateOrEditUserTokenDto : EntityDto<int?>
    {

        [Required]
        [StringLength(UserTokenConsts.MaxTokenLength, MinimumLength = UserTokenConsts.MinTokenLength)]
        public string Token { get; set; }

        public long? UserId { get; set; }

    }
}