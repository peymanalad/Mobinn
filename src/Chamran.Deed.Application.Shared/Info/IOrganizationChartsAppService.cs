using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Chamran.Deed.Info.Dtos;
using Chamran.Deed.Dto;

namespace Chamran.Deed.Info
{
    public interface IOrganizationChartsAppService : IApplicationService
    {
        Task<PagedResultDto<GetOrganizationChartForViewDto>> GetAll(GetAllOrganizationChartsInput input);

        Task<GetOrganizationChartForViewDto> GetOrganizationChartForView(int id);

        Task<GetOrganizationChartForEditOutput> GetOrganizationChartForEdit(EntityDto input);

        Task CreateOrEdit(CreateOrEditOrganizationChartDto input);

        Task Delete(EntityDto input);

        Task<PagedResultDto<OrganizationChartOrganizationChartLookupTableDto>> GetAllOrganizationChartForLookupTable(GetAllForLookupTableInput input);

        Task SetOrganizationForChartLeaf(SetOrganizationForChartLeafInput input);

    }
}