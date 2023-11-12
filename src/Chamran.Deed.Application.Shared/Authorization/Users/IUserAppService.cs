using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Chamran.Deed.Authorization.Users.Dto;
using Chamran.Deed.Dto;
using Chamran.Deed.People.Dtos;

namespace Chamran.Deed.Authorization.Users
{
    public interface IUserAppService : IApplicationService
    {
        Task<PagedResultDto<UserListDto>> GetUsers(GetUsersInput input);

        Task<FileDto> GetUsersToExcel(GetUsersToExcelInput input);

        Task<List<OrganizationDto>> GetListOfOrganizations(int userId);

        Task<GetUserForEditOutput> GetUserForEdit(NullableIdDto<long> input);

        Task<GetUserPermissionsForEditOutput> GetUserPermissionsForEdit(EntityDto<long> input);

        Task ResetUserSpecificPermissions(EntityDto<long> input);

        Task UpdateUserPermissions(UpdateUserPermissionsInput input);

        Task<long> CreateOrUpdateUser(CreateOrUpdateUserInput input);
        Task<long> CreateNode(CreateNodeDto input);

        Task DeleteUser(EntityDto<long> input);

        Task UnlockUser(EntityDto<long> input);
        Task RemoveProfilePicture(long userId);
    }
}