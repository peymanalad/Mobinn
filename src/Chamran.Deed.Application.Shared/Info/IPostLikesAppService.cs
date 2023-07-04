using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Chamran.Deed.Info.Dtos;
using Chamran.Deed.Dto;

namespace Chamran.Deed.Info
{
    public interface IPostLikesAppService : IApplicationService
    {
        Task<PagedResultDto<GetPostLikeForViewDto>> GetAll(GetAllPostLikesInput input);

        Task<GetPostLikeForViewDto> GetPostLikeForView(int id);

        Task<GetPostLikeForEditOutput> GetPostLikeForEdit(EntityDto input);

        Task CreateOrEdit(CreateOrEditPostLikeDto input);

        Task Delete(EntityDto input);

        Task<FileDto> GetPostLikesToExcel(GetAllPostLikesForExcelInput input);

        Task<PagedResultDto<PostLikePostLookupTableDto>> GetAllPostForLookupTable(GetAllForLookupTableInput input);

        Task<PagedResultDto<PostLikeUserLookupTableDto>> GetAllUserForLookupTable(GetAllForLookupTableInput input);

    }
}