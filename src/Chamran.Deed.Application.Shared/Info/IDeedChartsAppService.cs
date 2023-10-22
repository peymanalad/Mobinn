using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Chamran.Deed.Info.Dtos;
using Chamran.Deed.Dto;

namespace Chamran.Deed.Info
{
    public interface IDeedChartsAppService : IApplicationService
    {
        Task<PagedResultDto<GetDeedChartForViewDto>> GetAll(GetAllDeedChartsInput input);

        Task<GetDeedChartForViewDto> GetDeedChartForView(int id);

        Task<GetDeedChartForEditOutput> GetDeedChartForEdit(EntityDto input);

        Task CreateOrEdit(CreateOrEditDeedChartDto input);

        Task Delete(EntityDto input);

        Task<PagedResultDto<DeedChartOrganizationLookupTableDto>> GetAllOrganizationForLookupTable(GetAllForLookupTableInput input);

        Task<PagedResultDto<DeedChartDeedChartLookupTableDto>> GetAllDeedChartForLookupTable(GetAllForLookupTableInput input);

        Task SetOrganizationForChartLeaf(SetOrganizationForChartLeafInput input);
    }
}