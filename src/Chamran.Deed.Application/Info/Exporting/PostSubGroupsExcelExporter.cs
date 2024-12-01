using System.Collections.Generic;
using Abp.Runtime.Session;
using Abp.Timing.Timezone;
using Chamran.Deed.DataExporting.Excel.NPOI;
using Chamran.Deed.Info.Dtos;
using Chamran.Deed.Dto;
using Chamran.Deed.Storage;

namespace Chamran.Deed.Info.Exporting
{
    public class PostSubGroupsExcelExporter : NpoiExcelExporterBase, IPostSubGroupsExcelExporter
    {

        private readonly ITimeZoneConverter _timeZoneConverter;
        private readonly IAbpSession _abpSession;

        public PostSubGroupsExcelExporter(
            ITimeZoneConverter timeZoneConverter,
            IAbpSession abpSession,
            ITempFileCacheManager tempFileCacheManager) :
    base(tempFileCacheManager)
        {
            _timeZoneConverter = timeZoneConverter;
            _abpSession = abpSession;
        }

        public FileDto ExportToFile(List<GetPostSubGroupForViewDto> PostSubGroups)
        {
            return CreateExcelPackage(
                    "PostSubGroups.xlsx",
                    excelPackage =>
                    {

                        var sheet = excelPackage.CreateSheet(L("PostSubGroups"));

                        AddHeader(
                            sheet,
                        L("PostSubGroupDescription"),
                        L("GroupFile")
                            );

                        AddObjects(
                            sheet, PostSubGroups,
                        _ => _.PostSubGroup.PostSubGroupDescription,
                        _ => _.PostSubGroup.SubGroupFileFileName
                            );

                    });

        }
    }
}