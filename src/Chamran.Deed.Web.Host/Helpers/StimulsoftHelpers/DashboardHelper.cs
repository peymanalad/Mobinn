using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.Timing;
using Abp.UI;
using Chamran.Deed.Info;
using Chamran.Deed.People;
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
            IRepository<Organization> organizationRepository, IRepository<GroupMember> groupMemberRepository,
            long userId)
        {
            var query = reportRepository.GetAll().Where(x => x.IsDashboard && !x.IsDeleted);
            var query2 = from report in query
                         join grpMember in groupMemberRepository.GetAll() on report.OrganizationId equals grpMember.OrganizationId into joined2
                         from grpMember in joined2.DefaultIfEmpty()
                         where grpMember.UserId == userId && report.IsDashboard
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
                from org in organizationRepository.GetAll().Where(x => !x.IsDeleted)
                join grpMember in groupMemberRepository.GetAll() on org.Id equals grpMember
                    .OrganizationId into joined2
                from grpMember in joined2.DefaultIfEmpty()
                where grpMember.UserId == userId
                select org;

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
                CreationTime = Clock.Now,
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

        public static async Task SaveCurrentOrganizationDashboard(StiReport savedReport, IRepository<Report> reportRepository, IRepository<Organization> organizationRepository, IRepository<GroupMember> groupMemberRepository, long userId)
        {
            var query = reportRepository.GetAll().Where(x => x.IsDashboard && !x.IsDeleted);
            var query2 = from report in query
                         join org in organizationRepository.GetAll().Where(x => !x.IsDeleted) on report.OrganizationId equals org.Id into joined1
                         from org in joined1.DefaultIfEmpty()
                         join grpMember in groupMemberRepository.GetAll() on org.Id equals grpMember.OrganizationId into joined2
                         from grpMember in joined2.DefaultIfEmpty()
                         where grpMember.UserId == userId
                         select report;

            if (query2.Any())
            {
                var result = await query2.FirstAsync();
                result.ReportContent = savedReport.SaveEncryptedReportToString("DrM@s");
                result.LastModificationTime = Clock.Now;
                result.LastModifierUserId = userId;
                result.IsDashboard = true;

                await reportRepository.UpdateAsync(result);
            }
        }

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
                //UserID = "",
                //Password = ""
            };
            dashboard.Dictionary.Databases.Add(new StiSqlDatabase("DeedDb","DeedDb", sqlconbuilder.ConnectionString,false));
            //dashboard.Dictionary.Synchronize();
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
            dashboard.Dictionary.Databases.Add(new StiSqlDatabase("DeedDb","DeedDb" ,sqlconbuilder.ConnectionString, false));
            //dashboard.Dictionary.Synchronize();
        }

        //public static void BundleDataToReport(StiReport dashboard, IConfigurationRoot configuration)
        //{
        //foreach (var stiDatabase in dashboard.Dictionary.Databases.Items)
        //{
        //    if (stiDatabase.ConnectionType == StiConnectionType.Rest &&
        //        stiDatabase.ConnectionOrder == StiConnectionOrder.GraphQLDataSource)
        //    {
        //        Console.WriteLine(stiDatabase);

        //        var endpoint = ((StiGraphQLDatabase)stiDatabase).EndPoint;
        //        var query= ((StiGraphQLDatabase)stiDatabase).EndPoint;
        //        var enc=StiEncryption.Decrypt(((StiGraphQLDatabase)stiDatabase).ConnectionStringEncrypted, "8pTP5X15uKADcSw7");
        //        Console.WriteLine(enc);

        //    }
        //}
        //dashboard.Dictionary.Databases.Add(new StiSqlDatabase("DeedDb", sqlconbuilder.ConnectionString));
        //}




    }
}
