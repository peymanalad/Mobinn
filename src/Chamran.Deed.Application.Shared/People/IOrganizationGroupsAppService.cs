using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Chamran.Deed.People.Dtos;
using Chamran.Deed.Dto;

namespace Chamran.Deed.People
{
    public interface IOrganizationGroupsAppService : IApplicationService
    {
        Task<PagedResultDto<GetOrganizationGroupForViewDto>> GetAll(GetAllOrganizationGroupsInput input);

        Task<GetOrganizationGroupForViewDto> GetOrganizationGroupForView(int id);

        Task<GetOrganizationGroupForEditOutput> GetOrganizationGroupForEdit(EntityDto input);

        Task CreateOrEdit(CreateOrEditOrganizationGroupDto input);

        Task Delete(EntityDto input);

        Task<FileDto> GetOrganizationGroupsToExcel(GetAllOrganizationGroupsForExcelInput input);

        Task<PagedResultDto<OrganizationGroupOrganizationLookupTableDto>> GetAllOrganizationForLookupTable(GetAllForLookupTableInput input);

    }
}