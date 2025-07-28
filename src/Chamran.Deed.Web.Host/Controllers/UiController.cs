using System.Threading.Tasks;
using Abp.Auditing;
using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.Configuration.Startup;
using Abp.UI;
using Microsoft.AspNetCore.Mvc;
using Chamran.Deed.Authorization;
using Chamran.Deed.Authorization.Accounts;
using Chamran.Deed.Authorization.Accounts.Dto;
using Chamran.Deed.Authorization.Users;
using Chamran.Deed.Identity;
using Chamran.Deed.MultiTenancy;
using Chamran.Deed.Web.Models.Ui;
using Chamran.Deed.Web.Session;
using Microsoft.AspNetCore.Identity;
using static Chamran.Deed.Configuration.AppSettings.UserManagement;
using Chamran.Deed.Authorization.Roles;
using Abp.Domain.Repositories;
using Abp.MultiTenancy;
using Abp;
using System.Security.Claims;

namespace Chamran.Deed.Web.Controllers
{
    [IgnoreAntiforgeryToken]
    public class UiController : DeedControllerBase
    {
        private readonly IPerRequestSessionCache _sessionCache;
        private readonly IMultiTenancyConfig _multiTenancyConfig;
        private readonly IAccountAppService _accountAppService;
        private readonly LogInManager _logInManager;
        private readonly SignInManager _signInManager;
        private readonly AbpLoginResultTypeHelper _abpLoginResultTypeHelper;
        private readonly AbpUserClaimsPrincipalFactory<User, Role> _claimsPrincipalFactory;
        private readonly UserManager _userManager;
        private readonly IRepository<Tenant, int> _tenantRepository;

        public UiController(
            IPerRequestSessionCache sessionCache,
            IMultiTenancyConfig multiTenancyConfig,
            IAccountAppService accountAppService,
            LogInManager logInManager,
            SignInManager signInManager,
            AbpLoginResultTypeHelper abpLoginResultTypeHelper,
            AbpUserClaimsPrincipalFactory<User, Role> claimsPrincipalFactory,
            UserManager userManager,
            IRepository<Tenant, int> tenantRepository)
        {
            _sessionCache = sessionCache;
            _multiTenancyConfig = multiTenancyConfig;
            _accountAppService = accountAppService;
            _logInManager = logInManager;
            _signInManager = signInManager;
            _abpLoginResultTypeHelper = abpLoginResultTypeHelper;
            _claimsPrincipalFactory = claimsPrincipalFactory;
            _userManager = userManager;
            _tenantRepository = tenantRepository;
        }

        [DisableAuditing]
        public async Task<IActionResult> Index()
        {
            var model = new HomePageModel
            {
                LoginInformation = await _sessionCache.GetCurrentLoginInformationsAsync(),
                IsMultiTenancyEnabled = _multiTenancyConfig.IsEnabled
            };

            if (model.LoginInformation?.User == null)
            {
                return RedirectToAction("Login");
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Login(string returnUrl = "")
        {
            if (!string.IsNullOrEmpty(returnUrl))
            {
                ViewBag.ReturnUrl = returnUrl;
            }

            await _signInManager.SignOutAsync();
            
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model, string returnUrl = "")
        {
            if (model.TenancyName != null)
            {
                var isTenantAvailable = await _accountAppService.IsTenantAvailable(new IsTenantAvailableInput
                {
                    TenancyName = model.TenancyName
                });

                switch (isTenantAvailable.State)
                {
                    case TenantAvailabilityState.InActive:
                        throw new UserFriendlyException(L("TenantIsNotActive", model.TenancyName));
                    case TenantAvailabilityState.NotFound:
                        throw new UserFriendlyException(L("ThereIsNoTenantDefinedWithName{0}", model.TenancyName));
                }
            }
            AbpLoginResult<Tenant, User> loginResult =
                await GetLoginResultAsync(model.UserNameOrEmailAddress, model.Password, model.TenancyName);

            //if (loginResult.User.ShouldChangePasswordOnNextLogin)
            //{
            //    throw new UserFriendlyException(L("RequiresPasswordChange"));
            //}

            var signInResult = await _signInManager.SignInOrTwoFactorAsync(loginResult, model.RememberMe);

            if (signInResult.RequiresTwoFactor)
            {
                throw new UserFriendlyException(L("RequiresTwoFactorAuth"));
            }

            if (!string.IsNullOrEmpty(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction("Index");
        }

        private async Task<AbpLoginResult<Tenant, User>> GetLoginResultAsync(string usernameOrEmailAddress, string password, string tenancyName)
        {
            var loginResult = await _logInManager.LoginAsync(usernameOrEmailAddress, password, tenancyName);

            switch (loginResult.Result)
            {
                case AbpLoginResultType.Success:
                    return loginResult;
                default:
                    throw _abpLoginResultTypeHelper.CreateExceptionForFailedLoginAttempt(loginResult.Result, usernameOrEmailAddress, tenancyName);
            }
        }
    }
}
