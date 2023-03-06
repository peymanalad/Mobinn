using System.Collections.Generic;
using Abp.Runtime.Session;
using Abp.Timing.Timezone;
using Chamran.Deed.DataExporting.Excel.NPOI;
using Chamran.Deed.People.Dtos;
using Chamran.Deed.Dto;
using Chamran.Deed.Storage;

namespace Chamran.Deed.People.Exporting
{
    public class OrganizationGroupsExcelExporter : NpoiExcelExporterBase, IOrganizationGroupsExcelExporter
    {

        private readonly ITimeZoneConverter _timeZoneConverter;
        private readonly IAbpSession _abpSession;

        public OrganizationGroupsExcelExporter(
            ITimeZoneConverter timeZoneConverter,
            IAbpSession abpSession,
            ITempFileCacheManager tempFileCacheManager) :
    base(tempFileCacheManager)
        {
            _timeZoneConverter = timeZoneConverter;
            _abpSession = abpSession;
        }

        public FileDto ExportToFile(List<GetOrganizationGroupForViewDto> organizationGroups)
        {
            return CreateExcelPackage(
                "OrganizationGroups.xlsx",
                excelPackage =>
                {

                    var sheet = excelPackage.CreateSheet(L("OrganizationGroups"));

                    AddHeader(
                        sheet,
                        L("GroupName"),
                        (L("Organization")) + L("OrganizationName")
                        );

                    AddObjects(
                        sheet, organizationGroups,
                        _ => _.OrganizationGroup.GroupName,
                        _ => _.OrganizationOrganizationName
                        );

                });
        }
    }
}