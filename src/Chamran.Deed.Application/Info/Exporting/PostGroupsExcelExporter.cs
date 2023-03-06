using System.Collections.Generic;
using Abp.Runtime.Session;
using Abp.Timing.Timezone;
using Chamran.Deed.DataExporting.Excel.NPOI;
using Chamran.Deed.Info.Dtos;
using Chamran.Deed.Dto;
using Chamran.Deed.Storage;

namespace Chamran.Deed.Info.Exporting
{
    public class PostGroupsExcelExporter : NpoiExcelExporterBase, IPostGroupsExcelExporter
    {

        private readonly ITimeZoneConverter _timeZoneConverter;
        private readonly IAbpSession _abpSession;

        public PostGroupsExcelExporter(
            ITimeZoneConverter timeZoneConverter,
            IAbpSession abpSession,
            ITempFileCacheManager tempFileCacheManager) :
    base(tempFileCacheManager)
        {
            _timeZoneConverter = timeZoneConverter;
            _abpSession = abpSession;
        }

        public FileDto ExportToFile(List<GetPostGroupForViewDto> postGroups)
        {
            return CreateExcelPackage(
                "PostGroups.xlsx",
                excelPackage =>
                {

                    var sheet = excelPackage.CreateSheet(L("PostGroups"));

                    AddHeader(
                        sheet,
                        L("PostGroupDescription"),
                        (L("OrganizationGroup")) + L("GroupName")
                        );

                    AddObjects(
                        sheet, postGroups,
                        _ => _.PostGroup.PostGroupDescription,
                        _ => _.OrganizationGroupGroupName
                        );

                });
        }
    }
}