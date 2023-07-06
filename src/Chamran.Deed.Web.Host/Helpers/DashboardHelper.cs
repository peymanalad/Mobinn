using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Logging;
using Abp.UI;
using Chamran.Deed.Info;
using Chamran.Deed.People;
using Microsoft.EntityFrameworkCore;
using Stimulsoft.Base.Drawing;
using Stimulsoft.Dashboard.Components;
using Stimulsoft.Dashboard.Components.Table;
using Stimulsoft.Data.Engine;
using Stimulsoft.Report;
using Z.EntityFramework.Plus;

namespace Chamran.Deed.Web.Helpers
{
    public class DashboardHelper
    {
        /*public static StiReport CreateTemplate(string appPath)
        {
            var report = StiReport.CreateNewDashboard();
            var dashboard = report.Pages[0] as StiDashboard;

            //var dataPath = Path.Combine(appPath, "Data/Demo.xml");
            //var data = new DataSet();
            //data.ReadXml(dataPath);

            //report.RegData(data);
            report.Dictionary.Synchronize();

            var tableElement = new StiTableElement
            {
                Left = 0,
                Top = 0,
                Width = 700,
                Height = 500,
                Border =
                {
                    Side = StiBorderSides.All
                },
                BackColor = Color.LightGray,
                Name = "Example"
            };

            var dataBase = new StiDimensionColumn
            {
                Expression = "Products.ProductID"
            };
            tableElement.Columns.Add(dataBase);

            var dataBase1 = new StiDimensionColumn
            {
                Expression = "Products.ProductName"
            };
            tableElement.Columns.Add(dataBase1);

            var dataBase2 = new StiDimensionColumn
            {
                Expression = "Products.UnitPrice"
            };
            tableElement.Columns.Add(dataBase2);

            var filter1 = new StiDataFilterRule
            {
                Condition = StiDataFilterCondition.BeginningWith,
                Path = "Products.ProductID",
                Value = "1"
            };
            tableElement.DataFilters.Add(filter1);

            var filter2 = new StiDataFilterRule
            {
                Condition = StiDataFilterCondition.EndingWith,
                Path = "Products.UnitPrice",
                Value = "1"
            };
            tableElement.DataFilters.Add(filter2);

            dashboard?.Components.Add(tableElement);

            return report;
        }*/

        public static async Task<StiReport> GetCurrentOrganizationDashboard(IRepository<Report> reportRepository,
            IRepository<OrganizationGroup> organizationGroupRepository, IRepository<GroupMember> groupMemberRepository,
            long userId)
        {
            var query = reportRepository.GetAll().Where(x => x.IsDashboard);
            var query2 = from report in query
                         join orgGroup in organizationGroupRepository.GetAll() on report.OrganizationId equals orgGroup
                             .OrganizationId into joined1
                         from orgGroup in joined1.DefaultIfEmpty()
                         join grpMember in groupMemberRepository.GetAll() on orgGroup.OrganizationId equals grpMember
                             .OrganizationGroupId into joined2
                         from grpMember in joined2.DefaultIfEmpty()
                         where grpMember.UserId == userId && !orgGroup.IsDeleted && !report.IsDeleted 
                         select new
                         {
                             report.ReportContent
                         };
            string reportContent = null;
            if (query2.Any())
            {
                var result = await query2.FirstAsync();
                reportContent = result.ReportContent;

            }

            if (!string.IsNullOrEmpty(reportContent)) return GetReportFromContent(reportContent);
            var orgQuery =
                from orgGroup in organizationGroupRepository.GetAll().Include(orgGroup => orgGroup.OrganizationFk)
                join grpMember in groupMemberRepository.GetAll() on orgGroup.Id equals grpMember
                    .OrganizationGroupId into joined2
                from grpMember in joined2.DefaultIfEmpty()
                where grpMember.UserId == userId && !orgGroup.IsDeleted
                select orgGroup.OrganizationFk;

            if (!orgQuery.Any())
            {
                throw new UserFriendlyException("کاربر عضو هیچ گروهی در هیچ سازمانی نمی باشد");
            }
            return CreateEmptyReport(userId, orgQuery.First(), reportRepository);


        }

        private static StiReport GetReportFromContent(string reportContent)
        {
            try
            {
                var report = StiReport.CreateNewDashboard();
                report.LoadEncryptedReportFromString(reportContent, "DrM@s");
                return report;
            }
            catch (Exception)
            {
                //ignored
            }
            return GetTemplateDashboard();
            
        }

        private static StiReport CreateEmptyReport(long userId, Organization org, IRepository<Report> reportRepository)
        {
            var report = GetTemplateDashboard();
            reportRepository.Insert(new Report()
            {
                OrganizationId = org.Id,
                IsDashboard = true,
                ReportContent = report.SaveEncryptedReportToString("DrM@s"),
                CreationTime = DateTime.Now,
                CreatorUserId = userId,
                ReportDescription = "داشبورد سازمانی " + org.OrganizationName
            });

            return report;

        }

        private static StiReport GetTemplateDashboard()
        {

            var report = StiReport.CreateNewDashboard();
            var dashboard = report.Pages[0] as StiDashboard;

            //var dataPath = Path.Combine(appPath, "Data/Demo.xml");
            //var data = new DataSet();
            //data.ReadXml(dataPath);

            //report.RegData(data);
            report.Dictionary.Synchronize();

            var tableElement = new StiTableElement
            {
                Left = 0,
                Top = 0,
                Width = 700,
                Height = 500,
                Border =
                {
                    Side = StiBorderSides.All
                },
                BackColor = Color.LightGray,
                Name = "نمونه"
            };

            var dataBase = new StiDimensionColumn
            {
                Expression = "Products.ProductID"
            };
            tableElement.Columns.Add(dataBase);

            var dataBase1 = new StiDimensionColumn
            {
                Expression = "Products.ProductName"
            };
            tableElement.Columns.Add(dataBase1);

            var dataBase2 = new StiDimensionColumn
            {
                Expression = "Products.UnitPrice"
            };
            tableElement.Columns.Add(dataBase2);

            var filter1 = new StiDataFilterRule
            {
                Condition = StiDataFilterCondition.BeginningWith,
                Path = "Products.ProductID",
                Value = "1"
            };
            tableElement.DataFilters.Add(filter1);

            var filter2 = new StiDataFilterRule
            {
                Condition = StiDataFilterCondition.EndingWith,
                Path = "Products.UnitPrice",
                Value = "1"
            };
            tableElement.DataFilters.Add(filter2);

            dashboard?.Components.Add(tableElement);

            return report;
        }

        public static async Task SaveCurrentOrganizationDashboard(StiReport savedReport, IRepository<Report> reportRepository, IRepository<OrganizationGroup> organizationGroupRepository, IRepository<GroupMember> groupMemberRepository, long userId)
        {
                var query = reportRepository.GetAll().Where(x => x.IsDashboard);
                var query2 = from report in query
                    join orgGroup in organizationGroupRepository.GetAll() on report.OrganizationId equals orgGroup.OrganizationId into joined1
                    from orgGroup in joined1.DefaultIfEmpty()
                    join grpMember in groupMemberRepository.GetAll() on orgGroup.OrganizationId equals grpMember.OrganizationGroupId into joined2
                    from grpMember in joined2.DefaultIfEmpty()
                    where grpMember.UserId == userId && !orgGroup.IsDeleted && !report.IsDeleted
                    select report;

                if (query2.Any())
                {
                    var result = await query2.FirstAsync();
                    result.ReportContent = savedReport.SaveEncryptedReportToString("DrM@s");
                    result.LastModificationTime = DateTime.Now;
                    result.LastModifierUserId = userId;

                    await reportRepository.UpdateAsync(result);
                }

        }
    }
}
