using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace Chamran.Deed.Info.Dtos
{
    public class GetInstagramCrawlerPostForEditOutput
    {
        public CreateOrEditInstagramCrawlerPostDto InstagramCrawlerPost { get; set; }

    }
}