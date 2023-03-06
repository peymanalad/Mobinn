using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace Chamran.Deed.Info.Dtos
{
    public class CreateOrEditHashtagDto : EntityDto<int?>
    {

        public string HashtagTitle { get; set; }

        public int? PostId { get; set; }

    }
}