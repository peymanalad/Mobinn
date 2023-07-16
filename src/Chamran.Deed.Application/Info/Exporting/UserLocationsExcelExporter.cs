using System.Collections.Generic;
using Abp.Runtime.Session;
using Abp.Timing.Timezone;
using Chamran.Deed.DataExporting.Excel.NPOI;
using Chamran.Deed.Info.Dtos;
using Chamran.Deed.Dto;
using Chamran.Deed.Storage;

namespace Chamran.Deed.Info.Exporting
{
    public class UserLocationsExcelExporter : NpoiExcelExporterBase, IUserLocationsExcelExporter
    {

        private readonly ITimeZoneConverter _timeZoneConverter;
        private readonly IAbpSession _abpSession;

        public UserLocationsExcelExporter(
            ITimeZoneConverter timeZoneConverter,
            IAbpSession abpSession,
            ITempFileCacheManager tempFileCacheManager) :
    base(tempFileCacheManager)
        {
            _timeZoneConverter = timeZoneConverter;
            _abpSession = abpSession;
        }

        public FileDto ExportToFile(List<GetUserLocationForViewDto> userLocations)
        {
            return CreateExcelPackage(
                    "UserLocations.xlsx",
                    excelPackage =>
                    {

                        var sheet = excelPackage.CreateSheet(L("UserLocations"));

                        AddHeader(
                            sheet,
                        L("UserLat"),
                        L("UserLong"),
                        (L("User")) + L("Name")
                            );

                        AddObjects(
                            sheet, userLocations,
                        _ => _.UserLocation.UserLat,
                        _ => _.UserLocation.UserLong,
                        _ => _.UserName
                            );

                    });

        }
    }
}