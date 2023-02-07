using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Chamran.Deed.Authorization.Permissions.Dto;

namespace Chamran.Deed.Authorization.Permissions
{
    public interface IPermissionAppService : IApplicationService
    {
        ListResultDto<FlatPermissionWithLevelDto> GetAllPermissions();
    }
}
