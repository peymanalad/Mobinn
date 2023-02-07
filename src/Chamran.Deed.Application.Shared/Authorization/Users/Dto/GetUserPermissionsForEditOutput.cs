using System.Collections.Generic;
using Chamran.Deed.Authorization.Permissions.Dto;

namespace Chamran.Deed.Authorization.Users.Dto
{
    public class GetUserPermissionsForEditOutput
    {
        public List<FlatPermissionDto> Permissions { get; set; }

        public List<string> GrantedPermissionNames { get; set; }
    }
}