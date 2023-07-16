using Abp.Application.Services.Dto;

namespace Chamran.Deed.Info.Dtos
{
    public class GetExploreForViewDto
    {
        public PagedResultDto<GetPostCategoriesForViewDto> PostCategoriesForViewDto { get; set; }
        public PagedResultDto<GetPostsForViewDto> PostsForViewDto { get; set; }
    }
}