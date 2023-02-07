using Abp.Authorization;
using Chamran.Deed.Authorization.Roles;
using Chamran.Deed.Authorization.Users;

namespace Chamran.Deed.Authorization
{
    public class PermissionChecker : PermissionChecker<Role, User>
    {
        public PermissionChecker(UserManager userManager)
            : base(userManager)
        {

        }
    }
}
