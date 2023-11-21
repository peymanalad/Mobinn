using Abp.Application.Services.Dto;
using System;

namespace Chamran.Deed.Info.Dtos
{
    public class GetAllInstagramCrawlerPostsInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }

        public string PostCaptionFilter { get; set; }

        public string PageIdFilter { get; set; }

        public string File1UrlFilter { get; set; }

        public string File2UrlFilter { get; set; }

        public string File3UrlFilter { get; set; }

        public string File4UrlFilter { get; set; }

        public string File5UrlFilter { get; set; }

        public string File6UrlFilter { get; set; }

        public string File7UrlFilter { get; set; }

        public string File8UrlFilter { get; set; }

        public string File9UrlFilter { get; set; }

        public string File10UrlFilter { get; set; }

        public DateTime? MaxPostTimeFilter { get; set; }
        public DateTime? MinPostTimeFilter { get; set; }

        public string MediaIdFilter { get; set; }

    }
}