using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Chamran.Deed.Info.Dtos;
using Chamran.Deed.Dto;

namespace Chamran.Deed.Info
{
    public interface IInstagramCrawlerPostsAppService : IApplicationService
    {
        Task<PagedResultDto<GetInstagramCrawlerPostForViewDto>> GetAll(GetAllInstagramCrawlerPostsInput input);

        Task<GetInstagramCrawlerPostForViewDto> GetInstagramCrawlerPostForView(int id);

        Task<GetInstagramCrawlerPostForEditOutput> GetInstagramCrawlerPostForEdit(EntityDto input);

        Task CreateOrEdit(CreateOrEditInstagramCrawlerPostDto input);

        Task Delete(EntityDto input);

    }
}