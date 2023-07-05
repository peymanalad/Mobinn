using System;
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
using Abp.Domain.Repositories;
using Chamran.Deed.Info;
using Chamran.Deed.People;
using Abp.Domain.Uow;
using Stimulsoft.Report.Dictionary;
using Stimulsoft.Report.Web;
using Stimulsoft.Base;
using System.IO;

namespace Chamran.Deed.Web.Controllers.Stimulsoft
{
    [AbpMvcAuthorize(AppPermissions.Pages_Administration)]
    public class DashboardController : DeedControllerBase
    {
        private readonly IPerRequestSessionCache _sessionCache;
        private readonly IMultiTenancyConfig _multiTenancyConfig;
        private readonly IAccountAppService _accountAppService;
        private readonly LogInManager _logInManager;
        private readonly SignInManager _signInManager;
        private readonly AbpLoginResultTypeHelper _abpLoginResultTypeHelper;
        private readonly IRepository<Report> _reportRepository;
        private readonly IRepository<OrganizationGroup> _organizationGroupRepository;
        private readonly IRepository<GroupMember> _groupMemberRepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public DashboardController(
            IPerRequestSessionCache sessionCache,
            IMultiTenancyConfig multiTenancyConfig,
            IAccountAppService accountAppService,
            LogInManager logInManager,
            SignInManager signInManager,
            AbpLoginResultTypeHelper abpLoginResultTypeHelper,
            IRepository<Report> reportRepository,
            IRepository<OrganizationGroup> organizationGroupRepository,
            IRepository<GroupMember> groupMemberRepository,
            IUnitOfWorkManager unitOfWorkManager)
        {
            _sessionCache = sessionCache;
            _multiTenancyConfig = multiTenancyConfig;
            _accountAppService = accountAppService;
            _logInManager = logInManager;
            _signInManager = signInManager;
            _abpLoginResultTypeHelper = abpLoginResultTypeHelper;
            _reportRepository = reportRepository;
            _organizationGroupRepository = organizationGroupRepository;
            _groupMemberRepository = groupMemberRepository;
            _unitOfWorkManager = unitOfWorkManager;
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

        public Task<IActionResult> Design()
        {
            // Check if the user is logged in or redirect to the login page if not

            var options = new StiNetCoreDesignerOptions
            {
                Actions =
                {
                    GetReport = "GetDesignReport",
                    DesignerEvent = "DesignerEvent",
                    SaveReport = "SaveReport",
                },
                Behavior =
                {
                    //SaveReportAsMode = StiSaveMode.Hidden
                    ShowSaveDialog = false,

                },
                Appearance =
                {
                    ShowDialogsHelp = false,
                    ShowSystemFonts = false,

                },
                FileMenu =
                {
                    ShowAbout = false,
                    ShowExit = false,
                    ShowClose = false,
                    ShowFileMenuNewDashboard = true,
                    ShowFileMenuNewReport = false,
                    ShowHelp = false,
                    ShowInfo = false,

                },
                Dictionary =
                {
                    //PermissionSqlParameters = StiDesignerPermissions.View,
                    PermissionDataSources = StiDesignerPermissions.View,
                    PermissionDataConnections = StiDesignerPermissions.View,

                }
            };

            //// Loading and adding a font to resources
            //var fontPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot","Fonts", "IRANSans.ttf");
            //var fontContent = System.IO.File.ReadAllBytes(fontPath);
            //var resource = new StiResource("IRANSans", "IRANSans", false, StiResourceType.FontTtf, fontContent, false);
            //StiFontCollection.AddResourceFont(resource.Name, resource.Content, "ttf", resource.Alias);



            // Add the custom function
            StiFunctions.AddFunction(
                "Conversion",
                "ConvertToPersianCalendar",
                "ConvertToPersianCalendar",
                "Converts a Georgian date to a Persian date By DrMas",
                typeof(ConversionFunctions),
                typeof(string),
                "Returns the converted Persian date",
                new[] { typeof(DateTime) },
                new[] { "georgianDate" },
                new[] { "The Georgian date to convert" }
            );

            ViewBag.Options = options;

            return Task.FromResult<IActionResult>(View());
        }

        public async Task<IActionResult> GetReport()
        {
            //var appPath = StiNetCoreHelper.MapPath(this, string.Empty);
            var loginInformation = await _sessionCache.GetCurrentLoginInformationsAsync();
            var dashboard = await Helpers.DashboardHelper.GetCurrentOrganizationDashboard(_reportRepository, _organizationGroupRepository, _groupMemberRepository, loginInformation.User.Id);

            //ToDo:MapData to Dashboard

            return StiNetCoreViewer.GetReportResult(this, dashboard);
        }

        public async Task<IActionResult> GetDesignReport()
        {
            //var appPath = StiNetCoreHelper.MapPath(this, string.Empty);
            var loginInformation = await _sessionCache.GetCurrentLoginInformationsAsync();
            var dashboard = await Helpers.DashboardHelper.GetCurrentOrganizationDashboard(_reportRepository, _organizationGroupRepository, _groupMemberRepository, loginInformation.User.Id);

            //ToDo:MapData to Dashboard

            // Loading and adding a font to resources
            var fontPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Fonts", "IRANSans.ttf");
            var fontContent = await System.IO.File.ReadAllBytesAsync(fontPath);
            var resource = new StiResource("IRANSans", "IRANSans", false, StiResourceType.FontTtf, fontContent, false);
            if (!dashboard.Dictionary.Resources.Contains("IRANSans"))
                dashboard.Dictionary.Resources.Add(resource);

            fontPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Fonts", "IRANSans(FaNum)_Medium.ttf");
            fontContent = await System.IO.File.ReadAllBytesAsync(fontPath);
            resource = new StiResource("IRANSans(FaNum) Medium", "IRANSans(FaNum)", false, StiResourceType.FontTtf, fontContent, false);
            if (!dashboard.Dictionary.Resources.Contains("IRANSans(FaNum) Medium"))
                dashboard.Dictionary.Resources.Add(resource);




            return StiNetCoreDesigner.GetReportResult(this, dashboard);
        }

        public IActionResult ViewerEvent()
        {
            return StiNetCoreViewer.ViewerEventResult(this);
        }

        #region Designer Events
        public IActionResult DesignerEvent()
        {
            return StiNetCoreDesigner.DesignerEventResult(this);
        }

        public IActionResult OnGetDesignerEvent()
        {
            return StiNetCoreDesigner.DesignerEventResult(this);
        }

        public IActionResult OnPostDesignerEvent()
        {
            return StiNetCoreDesigner.DesignerEventResult(this);
        }

        #endregion

        public async Task<IActionResult> SaveReport()
        {
            var report = StiNetCoreDesigner.GetReportObject(this);
            var loginInformation = await _sessionCache.GetCurrentLoginInformationsAsync();

            using (var unitOfWork = _unitOfWorkManager.Begin())
            {
                await Helpers.DashboardHelper.SaveCurrentOrganizationDashboard(report, _reportRepository, _organizationGroupRepository, _groupMemberRepository, loginInformation.User.Id);

                await unitOfWork.CompleteAsync();
            }

            return await StiNetCoreDesigner.SaveReportResultAsync(this);
        }
    }
}