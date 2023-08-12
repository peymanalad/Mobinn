using System.Collections.Generic;
using Abp.Runtime.Session;
using Abp.Timing.Timezone;
using Chamran.Deed.DataExporting.Excel.NPOI;
using Chamran.Deed.Info.Dtos;
using Chamran.Deed.Dto;
using Chamran.Deed.Storage;

namespace Chamran.Deed.Info.Exporting
{
    public class OrganizationUsersExcelExporter : NpoiExcelExporterBase, IOrganizationUsersExcelExporter
    {

        private readonly ITimeZoneConverter _timeZoneConverter;
        private readonly IAbpSession _abpSession;

        public OrganizationUsersExcelExporter(
            ITimeZoneConverter timeZoneConverter,
            IAbpSession abpSession,
            ITempFileCacheManager tempFileCacheManager) :
    base(tempFileCacheManager)
        {
            _timeZoneConverter = timeZoneConverter;
            _abpSession = abpSession;
        }

        public FileDto ExportToFile(List<GetOrganizationUserForViewDto> organizationUsers)
        {
            return CreateExcelPackage(
                    "OrganizationUsers.xlsx",
                    excelPackage =>
                    {

                        var sheet = excelPackage.CreateSheet(L("OrganizationUsers"));

                        AddHeader(
                            sheet,
                        (L("User")) + L("Name"),
                        (L("OrganizationChart")) + L("Caption")
                            );

                        AddObjects(
                            sheet, organizationUsers,
                        _ => _.UserName,
                        _ => _.OrganizationChartCaption
                            );

                    });

        }
    }
}