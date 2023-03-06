using System;
using Abp.Application.Services.Dto;

namespace Chamran.Deed.Info.Dtos
{
    public class HashtagDto : EntityDto
    {
        public string HashtagTitle { get; set; }

        public int? PostId { get; set; }

    }
}