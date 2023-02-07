using Abp.AspNetCore.Mvc.Authorization;
using Chamran.Deed.Authorization.Users.Profile;
using Chamran.Deed.Graphics;
using Chamran.Deed.Storage;

namespace Chamran.Deed.Web.Controllers
{
    [AbpMvcAuthorize]
    public class ProfileController : ProfileControllerBase
    {
        public ProfileController(
            ITempFileCacheManager tempFileCacheManager,
            IProfileAppService profileAppService,
            IImageFormatValidator imageFormatValidator) :
            base(tempFileCacheManager, profileAppService, imageFormatValidator)
        {
        }
    }
}