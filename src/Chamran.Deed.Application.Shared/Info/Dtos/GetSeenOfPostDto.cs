using System;
using Abp.Application.Services.Dto;

namespace Chamran.Deed.Info.Dtos
{
    public class GetSeenOfPostDto: PagedAndSortedResultRequestDto
    {
        public string FullName { get; set; }
        public Guid? ProfilePictureId { get; set; }


    }
}