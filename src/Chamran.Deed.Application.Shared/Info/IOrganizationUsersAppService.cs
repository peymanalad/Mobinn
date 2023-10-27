using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Chamran.Deed.Info.Dtos;
using Chamran.Deed.Dto;

namespace Chamran.Deed.Info
{
    public interface IOrganizationUsersAppService : IApplicationService
    {
        Task<PagedResultDto<GetOrganizationUserForViewDto>> GetAll(GetAllOrganizationUsersInput input);

        Task<GetOrganizationUserForViewDto> GetOrganizationUserForView(int id);

        Task<GetOrganizationUserForEditOutput> GetOrganizationUserForEdit(EntityDto input);

        Task CreateOrEdit(CreateOrEditOrganizationUserDto input);
        Task CreateGlobal(CreateOrEditGlobalUserDto input);
        Task Delete(EntityDto input);

        Task<FileDto> GetOrganizationUsersToExcel(GetAllOrganizationUsersForExcelInput input);

        Task<PagedResultDto<OrganizationUserUserLookupTableDto>> GetAllUserForLookupTable(GetAllForLookupTableInput input);

        Task<PagedResultDto<OrganizationUserOrganizationChartLookupTableDto>> GetAllOrganizationChartForLookupTable(GetAllForLookupTableInput input);

        Task<PagedResultDto<LeafUserDto>> GetUsersInSameLeaf(GetLeavesInput input);
        Task<PagedResultDto<LeafUserDto>> GetGlobalUserLeaves(GetLeavesInput input);
        Task<PagedResultDto<LeafUserDto>> GetUsersInOneLevelHigherParent(GetLeavesInput input);
        Task<PagedResultDto<LeafUserDto>> GetUsersInChildrenLeaves(GetLeavesInput input);
        Task<PagedResultDto<LeafUserDto>> GetAllUsersForLeaf(GetAllUsersForLeafInput input);
        Task<PagedResultDto<SameLeafDto>> GetAllUsersInLeaf(GetAllUsersInLeafInput input);

    }
}