using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Chamran.Deed.Common.Dtos;
using Chamran.Deed.Dto;

namespace Chamran.Deed.Common
{
    public interface IFCMQueuesAppService : IApplicationService
    {
        Task<PagedResultDto<GetFCMQueueForViewDto>> GetAll(GetAllFCMQueuesInput input);

        Task<GetFCMQueueForViewDto> GetFCMQueueForView(int id);

        Task<GetFCMQueueForEditOutput> GetFCMQueueForEdit(EntityDto input);

        Task CreateOrEdit(CreateOrEditFCMQueueDto input);

        Task Delete(EntityDto input);

    }
}