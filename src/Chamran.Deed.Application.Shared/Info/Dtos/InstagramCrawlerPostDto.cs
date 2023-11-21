using System;
using Abp.Application.Services.Dto;

namespace Chamran.Deed.Info.Dtos
{
    public class InstagramCrawlerPostDto : EntityDto
    {
        public string PostCaption { get; set; }

        public string PageId { get; set; }

        public string File1Url { get; set; }

        public string File2Url { get; set; }

        public string File3Url { get; set; }

        public string File4Url { get; set; }

        public string File5Url { get; set; }

        public string File6Url { get; set; }

        public string File7Url { get; set; }

        public string File8Url { get; set; }

        public string File9Url { get; set; }

        public string File10Url { get; set; }

        public DateTime? PostTime { get; set; }

        public string MediaId { get; set; }

    }
}