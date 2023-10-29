using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.Timing;
using Chamran.Deed.Info;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Stimulsoft.Base.Drawing;
using Stimulsoft.Dashboard.Components;
using Stimulsoft.Dashboard.Components.Table;
using Stimulsoft.Data.Engine;
using Stimulsoft.Report;
using Stimulsoft.Report.Dictionary;

namespace Chamran.Deed.Web.Helpers.StimulsoftHelpers
{
    public class ReportHelper
    {
        public static void MapDataToReportNoPassword(StiReport dashboard, IConfigurationRoot _appConfiguration)
        {
            foreach (var stiDatabase in dashboard.Dictionary.Databases.Items)
            {
                if (stiDatabase.Name == "DeedDb")
                    dashboard.Dictionary.Databases.Remove(stiDatabase);
            }

            var cn = _appConfiguration[$"ConnectionStrings:{DeedConsts.ConnectionStringName}"];
            var sqlconbuilder = new SqlConnectionStringBuilder(cn)
            {
                ConnectTimeout = 180,

            };
            dashboard.Dictionary.Databases.Add(new StiSqlDatabase("DeedDb", "DeedDb", sqlconbuilder.ConnectionString,
                false));

        }

        public static void MapDataToReportWithPassword(StiReport dashboard, IConfigurationRoot _appConfiguration)
        {
            foreach (var stiDatabase in dashboard.Dictionary.Databases.Items)
            {
                if (stiDatabase.Name == "DeedDb")
                    dashboard.Dictionary.Databases.Remove(stiDatabase);
            }

            var cn = _appConfiguration[$"ConnectionStrings:{DeedConsts.ConnectionStringName}"];
            var sqlconbuilder = new SqlConnectionStringBuilder(cn)
            {
                ConnectTimeout = 180,
            };
            dashboard.Dictionary.Databases.Add(new StiSqlDatabase("DeedDb", "DeedDb", sqlconbuilder.ConnectionString,
                false));

        }

        private static StiReport GetReportFromContent(string reportContent)
        {
            var report = StiReport.CreateNewReport();
            report.LoadEncryptedReportFromString(reportContent, "DrM@s");
            return report;


        }

        private static StiReport GetTemplateReport()
        {

            var report = StiReport.CreateNewReport();
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


        public static async Task<StiReport> GetReportById(IRepository<Report> reportRepository, int? reportId,
            long userId, bool userIsSuperUser)
        {
            try
            {
                if (reportId.HasValue)
                    return GetReportFromContent((await reportRepository.GetAsync(reportId.Value)).ReportContent);
            }
            catch (Exception)
            {
                //ignored
            }
            return GetTemplateReport();

        }

        public static async Task SaveReport(StiReport savedReport, int? reportId, int? organizationId,
            IRepository<Report> reportRepository, long userId, bool isSuperUser)
        {
            var query = reportRepository.GetAll().Where(x => x.Id == reportId);
            if (query.Any())
            {
                var result = await query.FirstAsync();
                result.ReportContent = savedReport.SaveEncryptedReportToString("DrM@s");
                result.LastModificationTime = Clock.Now;
                result.LastModifierUserId = userId;
                result.IsDashboard = false;
                result.IsSuperUser = isSuperUser;
                result.ReportDescription = savedReport.ReportDescription;
                result.OrganizationId=organizationId;
                await reportRepository.UpdateAsync(result);
            }
            else
            {
                var result = new Report
                {
                    ReportContent = savedReport.SaveEncryptedReportToString("DrM@s"),
                    LastModificationTime = Clock.Now,
                    LastModifierUserId = userId,
                    IsDashboard = false,
                    OrganizationId = organizationId,
                    ReportDescription = savedReport.ReportDescription,
                    IsSuperUser = isSuperUser,
                };
                await reportRepository.InsertAsync(result);
            }

        }
    }
}
