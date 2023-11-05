using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Chamran.Deed.Info.Dtos;
using Chamran.Deed.Dto;

namespace Chamran.Deed.Info
{
    public interface ITaskEntriesAppService : IApplicationService
    {
        Task<PagedResultDto<GetEntriesDigestDto>> GetEntriesDigest(GetEntriesDigestInputDto input);

        Task<PagedResultDto<GetEntriesDetailDto>> GetEntriesBySharedMessageId(GetEntriesBySharedMessageIdInputDto input);

        Task<PagedResultDto<GetTaskEntryForViewDto>> GetAll(GetAllTaskEntriesInput input);

        Task<GetTaskEntryForEditOutput> GetTaskEntryForEdit(EntityDto input);

        Task CreateOrEdit(CreateOrEditTaskEntryDto input);

        Task Delete(EntityDto input);

        Task<PagedResultDto<TaskEntryPostLookupTableDto>> GetAllPostForLookupTable(GetAllForLookupTableInput input);

        Task<PagedResultDto<TaskEntryUserLookupTableDto>> GetAllUserForLookupTable(GetAllForLookupTableInput input);

        Task<PagedResultDto<TaskEntryTaskEntryLookupTableDto>> GetAllTaskEntryForLookupTable(GetAllForLookupTableInput input);

        Task<TaskEntryDto?> GetPreviousTaskById(int id);

    }
}