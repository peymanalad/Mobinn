using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Chamran.Deed.Info.Dtos;
using Chamran.Deed.Dto;

namespace Chamran.Deed.Info
{
    public interface ITaskStatsAppService : IApplicationService
    {
        Task<PagedResultDto<GetTaskStatForViewDto>> GetAll(GetAllTaskStatsInput input);

        Task<GetTaskStatForEditOutput> GetTaskStatForEdit(EntityDto input);

        Task CreateOrEdit(CreateOrEditTaskStatDto input);

        Task Delete(EntityDto input);

        Task<PagedResultDto<TaskStatUserLookupTableDto>> GetAllUserForLookupTable(GetAllForLookupTableInput input);

        Task<GetTaskStatDto> GetTaskStat(Guid sharedTaskId);
    }
}