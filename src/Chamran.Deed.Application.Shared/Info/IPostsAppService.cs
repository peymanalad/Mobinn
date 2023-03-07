using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Chamran.Deed.Info.Dtos;
using Chamran.Deed.Dto;

namespace Chamran.Deed.Info
{
    public interface IPostsAppService : IApplicationService
    {
        Task<PagedResultDto<GetPostForViewDto>> GetAll(GetAllPostsInput input);

        Task<GetPostForViewDto> GetPostForView(int id);

        Task<GetPostForEditOutput> GetPostForEdit(EntityDto input);

        Task CreateOrEdit(CreateOrEditPostDto input);

        Task Delete(EntityDto input);

        Task<FileDto> GetPostsToExcel(GetAllPostsForExcelInput input);

        Task<PagedResultDto<PostGroupMemberLookupTableDto>> GetAllGroupMemberForLookupTable(GetAllForLookupTableInput input);

        Task<PagedResultDto<PostPostGroupLookupTableDto>> GetAllPostGroupForLookupTable(GetAllForLookupTableInput input);

        Task RemovePostFileFile(EntityDto input);

        Task<PagedResultDto<GetPostCategoriesForViewDto>> GetPostCategoriesForView();

    }
}