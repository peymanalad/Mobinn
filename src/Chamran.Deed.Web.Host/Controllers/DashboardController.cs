using Abp.AspNetCore.Mvc.Authorization;
using Abp.Auditing;
using Abp.Configuration.Startup;
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
using Stimulsoft.Report.Mvc;

namespace Chamran.Deed.Web.Controllers;

[AbpMvcAuthorize(AppPermissions.Pages_Administration)]
public class DashboardController : DeedControllerBase
{
    private readonly IPerRequestSessionCache _sessionCache;
    private readonly IMultiTenancyConfig _multiTenancyConfig;
    private readonly IAccountAppService _accountAppService;
    private readonly LogInManager _logInManager;
    private readonly SignInManager _signInManager;
    private readonly AbpLoginResultTypeHelper _abpLoginResultTypeHelper;


    public DashboardController(
        IPerRequestSessionCache sessionCache,
        IMultiTenancyConfig multiTenancyConfig,
        IAccountAppService accountAppService,
        LogInManager logInManager,
        SignInManager signInManager,
        AbpLoginResultTypeHelper abpLoginResultTypeHelper)
    {
        _sessionCache = sessionCache;
        _multiTenancyConfig = multiTenancyConfig;
        _accountAppService = accountAppService;
        _logInManager = logInManager;
        _signInManager = signInManager;
        _abpLoginResultTypeHelper = abpLoginResultTypeHelper;
    }
    // GET
    [DisableAuditing]
    public async Task<IActionResult> Index()
    {
        var model = new HomePageModel
        {
            LoginInformation = await _sessionCache.GetCurrentLoginInformationsAsync(),
            IsMultiTenancyEnabled = _multiTenancyConfig.IsEnabled
        };

        //if (model.LoginInformation?.User == null)
        //{
        //    return RedirectToAction("Login");
        //}

        return View(model);
    }

    public IActionResult GetReport()
    {
        var appPath = StiNetCoreHelper.MapPath(this, string.Empty);
        var dashboard = Helpers.Dashboard.CreateTemplate(appPath);
        return StiNetCoreViewer.GetReportResult(this, dashboard);
    }

    public IActionResult ViewerEvent()
    {
        return StiNetCoreViewer.ViewerEventResult(this);
    }
}