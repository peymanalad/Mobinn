using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Chamran.Deed.Common.Dtos;
using Chamran.Deed.Dto;

namespace Chamran.Deed.Common
{
    public interface ISoftwareUpdatesAppService : IApplicationService
    {
        Task<PagedResultDto<GetSoftwareUpdateForViewDto>> GetAll(GetAllSoftwareUpdatesInput input);

        Task<GetSoftwareUpdateForViewDto> GetSoftwareUpdateForView(int id);

        Task<GetSoftwareUpdateForEditOutput> GetSoftwareUpdateForEdit(EntityDto input);

        Task CreateOrEdit(CreateOrEditSoftwareUpdateDto input);

        Task Delete(EntityDto input);

        Task RemoveUpdateFileFile(EntityDto input);

        Task<GetSoftwareUpdateForViewDto> GetLatestUpdateInformation();

    }
}