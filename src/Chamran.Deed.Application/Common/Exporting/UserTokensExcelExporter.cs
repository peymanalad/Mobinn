using System.Collections.Generic;
using Abp.Runtime.Session;
using Abp.Timing.Timezone;
using Chamran.Deed.DataExporting.Excel.NPOI;
using Chamran.Deed.Common.Dtos;
using Chamran.Deed.Dto;
using Chamran.Deed.Storage;

namespace Chamran.Deed.Common.Exporting
{
    public class UserTokensExcelExporter : NpoiExcelExporterBase, IUserTokensExcelExporter
    {

        private readonly ITimeZoneConverter _timeZoneConverter;
        private readonly IAbpSession _abpSession;

        public UserTokensExcelExporter(
            ITimeZoneConverter timeZoneConverter,
            IAbpSession abpSession,
            ITempFileCacheManager tempFileCacheManager) :
    base(tempFileCacheManager)
        {
            _timeZoneConverter = timeZoneConverter;
            _abpSession = abpSession;
        }

        public FileDto ExportToFile(List<GetUserTokenForViewDto> userTokens)
        {
            return CreateExcelPackage(
                    "UserTokens.xlsx",
                    excelPackage =>
                    {

                        var sheet = excelPackage.CreateSheet(L("UserTokens"));

                        AddHeader(
                            sheet,
                        L("Token"),
                        (L("User")) + L("Name")
                            );

                        AddObjects(
                            sheet, userTokens,
                        _ => _.UserToken.Token,
                        _ => _.UserName
                            );

                    });

        }
    }
}