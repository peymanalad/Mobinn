using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace Chamran.Deed.Common.Dtos
{
    public class GetUserTokenForEditOutput
    {
        public CreateOrEditUserTokenDto UserToken { get; set; }

        public string UserName { get; set; }

    }
}