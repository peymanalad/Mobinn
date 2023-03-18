using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Chamran.Deed.Dto;
using Chamran.Deed.Info.Dtos;
using Chamran.Deed.Sessions;
using Chamran.Deed.Sessions.Dto;

namespace Chamran.Deed.Info
{
    public class ProxyPostsAppService : ProxyAppServiceBase, IPostsAppService
    {
     

        public Task<PagedResultDto<GetPostForViewDto>> GetAll(GetAllPostsInput input)
        {
            throw new System.NotImplementedException();
        }

        public Task<GetPostForViewDto> GetPostForView(int id)
        {
            throw new System.NotImplementedException();
        }

        public Task<GetPostForEditOutput> GetPostForEdit(EntityDto input)
        {
            throw new System.NotImplementedException();
        }

        public Task CreateOrEdit(CreateOrEditPostDto input)
        {
            throw new System.NotImplementedException();
        }

        public Task Delete(EntityDto input)
        {
            throw new System.NotImplementedException();
        }

        public Task<FileDto> GetPostsToExcel(GetAllPostsForExcelInput input)
        {
            throw new System.NotImplementedException();
        }

        public Task<PagedResultDto<PostGroupMemberLookupTableDto>> GetAllGroupMemberForLookupTable(GetAllForLookupTableInput input)
        {
            throw new System.NotImplementedException();
        }

        public Task<PagedResultDto<PostPostGroupLookupTableDto>> GetAllPostGroupForLookupTable(GetAllForLookupTableInput input)
        {
            throw new System.NotImplementedException();
        }

        public Task RemovePostFileFile(EntityDto input)
        {
            throw new System.NotImplementedException();
        }

        public async Task<PagedResultDto<GetPostCategoriesForViewDto>> GetPostCategoriesForView()
        {
            return await ApiClient.GetAsync<PagedResultDto<GetPostCategoriesForViewDto>>(GetEndpoint(nameof(GetPostCategoriesForView)));

        }

        public async Task<PagedResultDto<GetPostsForViewDto>> GetPostsForView(int postGroupId)
        {
            return await ApiClient.GetAsync<PagedResultDto<GetPostsForViewDto>>(GetEndpoint(nameof(GetPostsForView)), new { postGroupId = postGroupId });

        }
    }
}