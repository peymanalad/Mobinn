using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Chamran.Deed.Info.Dtos;

namespace Chamran.Deed.Info
{
    public interface ISeensAppService : IApplicationService
    {

        Task CreateCurrentSeen(int postId);

        Task CreateSeenByDate(int postId, DateTime seenDateTime);

        Task<PagedResultDto<GetSeenForViewDto>> GetAll(GetAllSeensInput input);
        Task<PagedResultDto<GetSeenOfPostDto>> GetSeensOfPost(GetSeensOfPostInput input);
        Task<PagedResultDto<GetSeenOfPostDto>> GetSeensOfPostFiltered(GetSeensOfPostFilteredInput input);


        Task<GetSeenForViewDto> GetSeenForView(int id);

        Task<GetSeenForEditOutput> GetSeenForEdit(EntityDto input);

        Task CreateOrEdit(CreateOrEditSeenDto input);

        Task Delete(EntityDto input);

        Task<PagedResultDto<SeenPostLookupTableDto>> GetAllPostForLookupTable(GetAllForLookupTableInput input);

        Task<PagedResultDto<SeenUserLookupTableDto>> GetAllUserForLookupTable(GetAllForLookupTableInput input);

    }
}