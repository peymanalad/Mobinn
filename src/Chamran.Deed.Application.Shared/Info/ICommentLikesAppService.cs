using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Chamran.Deed.Info.Dtos;
using Chamran.Deed.Dto;

namespace Chamran.Deed.Info
{
    public interface ICommentLikesAppService : IApplicationService
    {
        Task<PagedResultDto<GetCommentLikeForViewDto>> GetAll(GetAllCommentLikesInput input);

        Task<GetCommentLikeForViewDto> GetCommentLikeForView(int id);

        Task<GetCommentLikeForEditOutput> GetCommentLikeForEdit(EntityDto input);

        Task CreateOrEdit(CreateOrEditCommentLikeDto input);

        Task Delete(EntityDto input);

        Task<FileDto> GetCommentLikesToExcel(GetAllCommentLikesForExcelInput input);

        Task<PagedResultDto<CommentLikeCommentLookupTableDto>> GetAllCommentForLookupTable(GetAllForLookupTableInput input);

        Task<PagedResultDto<CommentLikeUserLookupTableDto>> GetAllUserForLookupTable(GetAllForLookupTableInput input);

    }
}