using System.Collections.Generic;
using Abp.Runtime.Session;
using Abp.Timing.Timezone;
using Chamran.Deed.DataExporting.Excel.NPOI;
using Chamran.Deed.Info.Dtos;
using Chamran.Deed.Dto;
using Chamran.Deed.Storage;

namespace Chamran.Deed.Info.Exporting
{
    public class ReportsExcelExporter : NpoiExcelExporterBase, IReportsExcelExporter
    {

        private readonly ITimeZoneConverter _timeZoneConverter;
        private readonly IAbpSession _abpSession;

        public ReportsExcelExporter(
            ITimeZoneConverter timeZoneConverter,
            IAbpSession abpSession,
            ITempFileCacheManager tempFileCacheManager) :
    base(tempFileCacheManager)
        {
            _timeZoneConverter = timeZoneConverter;
            _abpSession = abpSession;
        }

        public FileDto ExportToFile(List<GetReportForViewDto> reports)
        {
            return CreateExcelPackage(
                "Reports.xlsx",
                excelPackage =>
                {

                    var sheet = excelPackage.CreateSheet(L("Reports"));

                    AddHeader(
                        sheet,
                        L("ReportDescription"),
                        L("IsDashboard"),
                        (L("Organization")) + L("OrganizationName")
                        );

                    AddObjects(
                        sheet, reports,
                        _ => _.Report.ReportDescription,
                        _ => _.Report.IsDashboard,
                        _ => _.OrganizationOrganizationName
                        );

                });
        }
    }
}