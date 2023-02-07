using Abp.AspNetCore.Mvc.Authorization;
using Chamran.Deed.Authorization;
using Chamran.Deed.Storage;
using Abp.BackgroundJobs;

namespace Chamran.Deed.Web.Controllers
{
    [AbpMvcAuthorize(AppPermissions.Pages_Administration_Users)]
    public class UsersController : UsersControllerBase
    {
        public UsersController(IBinaryObjectManager binaryObjectManager, IBackgroundJobManager backgroundJobManager)
            : base(binaryObjectManager, backgroundJobManager)
        {
        }
    }
}