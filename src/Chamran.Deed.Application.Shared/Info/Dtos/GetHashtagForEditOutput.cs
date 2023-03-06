using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace Chamran.Deed.Info.Dtos
{
    public class GetHashtagForEditOutput
    {
        public CreateOrEditHashtagDto Hashtag { get; set; }

        public string PostPostTitle { get; set; }

    }
}