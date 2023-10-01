using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Chamran.Deed.People.Dtos;
using Chamran.Deed.Dto;

namespace Chamran.Deed.People
{
    public interface IGroupMembersAppService : IApplicationService
    {
        Task<PagedResultDto<GetGroupMemberForViewDto>> GetAll(GetAllGroupMembersInput input);
        Task<PagedResultDto<GetGroupMemberForViewDto>> GetAllNoOrganization(GetAllGroupMembersInput input);

        Task<GetGroupMemberForViewDto> GetGroupMemberForView(int id);

        Task<GetGroupMemberForEditOutput> GetGroupMemberForEdit(EntityDto input);

        Task CreateOrEdit(CreateOrEditGroupMemberDto input);

        Task Delete(EntityDto input);

        Task<FileDto> GetGroupMembersToExcel(GetAllGroupMembersForExcelInput input);

        Task<PagedResultDto<GroupMemberUserLookupTableDto>> GetAllUserForLookupTable(GetAllForLookupTableInput input);

        Task<PagedResultDto<GroupMemberOrganizationGroupLookupTableDto>> GetAllOrganizationGroupForLookupTable(GetAllForLookupTableInput input);

    }
}