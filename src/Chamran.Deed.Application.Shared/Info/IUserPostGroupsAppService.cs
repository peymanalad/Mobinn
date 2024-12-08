using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Chamran.Deed.Info.Dtos;
using Chamran.Deed.Dto;

namespace Chamran.Deed.Info
{
    public interface IUserPostGroupsAppService : IApplicationService
    {
        Task<PagedResultDto<GetUserPostGroupForViewDto>> GetAll(GetAllUserPostGroupsInput input);

        Task<GetUserPostGroupForViewDto> GetUserPostGroupForView(int id);

        Task<GetUserPostGroupForEditOutput> GetUserPostGroupForEdit(EntityDto input);

        Task CreateOrEdit(CreateOrEditUserPostGroupDto input);

        Task Delete(EntityDto input);

        Task<FileDto> GetUserPostGroupsToExcel(GetAllUserPostGroupsForExcelInput input);

        Task<PagedResultDto<UserPostGroupUserLookupTableDto>> GetAllUserForLookupTable(GetAllForLookupTableInput input);

        Task<PagedResultDto<UserPostGroupPostGroupLookupTableDto>> GetAllPostGroupForLookupTable(GetAllForLookupTableInput input);

        Task<PagedResultDto<UserPostGroupSelectDto>> GetUserPostGroupSelection(GetUserPostGroupSelectInput input);

        Task UpdateUserGroupSelection(List<int> input);

    }

    public interface IAllowedUserPostGroupsAppService : IApplicationService
    {
        Task<PagedResultDto<GetAllowedUserPostGroupForViewDto>> GetAll(GetAllAllowedUserPostGroupsInput input);

        Task<GetAllowedUserPostGroupForViewDto> GetAllowedUserPostGroupForView(int id);

        Task<GetAllowedUserPostGroupForEditOutput> GetAllowedUserPostGroupForEdit(EntityDto input);

        Task CreateOrEdit(CreateOrEditAllowedUserPostGroupDto input);

        Task Delete(EntityDto input);

        Task<FileDto> GetAllowedUserPostGroupsToExcel(GetAllAllowedUserPostGroupsForExcelInput input);

        Task<PagedResultDto<AllowedUserPostGroupUserLookupTableDto>> GetAllAllowedUserForLookupTable(GetAllForLookupTableInput input);

        Task<PagedResultDto<AllowedUserPostGroupPostGroupLookupTableDto>> GetAllAllowedPostGroupForLookupTable(GetAllForLookupTableInput input);

        Task<PagedResultDto<AllowedUserPostGroupselectDto>> GetAllowedUserPostGroupselection(GetAllowedUserPostGroupselectInput input);

        Task UpdateAllowedUserGroupSelection(List<int> input);

    }
}