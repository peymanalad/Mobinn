using System.Collections.Generic;
using Abp.Runtime.Session;
using Abp.Timing.Timezone;
using Chamran.Deed.DataExporting.Excel.NPOI;
using Chamran.Deed.Info.Dtos;
using Chamran.Deed.Dto;
using Chamran.Deed.Storage;

namespace Chamran.Deed.Info.Exporting
{
    public class UserPostGroupsExcelExporter : NpoiExcelExporterBase, IUserPostGroupsExcelExporter
    {

        private readonly ITimeZoneConverter _timeZoneConverter;
        private readonly IAbpSession _abpSession;

        public UserPostGroupsExcelExporter(
            ITimeZoneConverter timeZoneConverter,
            IAbpSession abpSession,
            ITempFileCacheManager tempFileCacheManager) :
    base(tempFileCacheManager)
        {
            _timeZoneConverter = timeZoneConverter;
            _abpSession = abpSession;
        }

        public FileDto ExportToFile(List<GetUserPostGroupForViewDto> userPostGroups)
        {
            return CreateExcelPackage(
                    "UserPostGroups.xlsx",
                    excelPackage =>
                    {

                        var sheet = excelPackage.CreateSheet(L("UserPostGroups"));

                        AddHeader(
                            sheet,
                        (L("User")) + L("Name"),
                        (L("PostGroup")) + L("PostGroupDescription")
                            );

                        AddObjects(
                            sheet, userPostGroups,
                        _ => _.UserName,
                        _ => _.PostGroupPostGroupDescription
                            );

                    });

        }
    }
}