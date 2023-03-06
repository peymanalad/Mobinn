using System.Collections.Generic;
using Abp.Runtime.Session;
using Abp.Timing.Timezone;
using Chamran.Deed.DataExporting.Excel.NPOI;
using Chamran.Deed.Info.Dtos;
using Chamran.Deed.Dto;
using Chamran.Deed.Storage;

namespace Chamran.Deed.Info.Exporting
{
    public class HashtagsExcelExporter : NpoiExcelExporterBase, IHashtagsExcelExporter
    {

        private readonly ITimeZoneConverter _timeZoneConverter;
        private readonly IAbpSession _abpSession;

        public HashtagsExcelExporter(
            ITimeZoneConverter timeZoneConverter,
            IAbpSession abpSession,
            ITempFileCacheManager tempFileCacheManager) :
    base(tempFileCacheManager)
        {
            _timeZoneConverter = timeZoneConverter;
            _abpSession = abpSession;
        }

        public FileDto ExportToFile(List<GetHashtagForViewDto> hashtags)
        {
            return CreateExcelPackage(
                "Hashtags.xlsx",
                excelPackage =>
                {

                    var sheet = excelPackage.CreateSheet(L("Hashtags"));

                    AddHeader(
                        sheet,
                        L("HashtagTitle"),
                        (L("Post")) + L("PostTitle")
                        );

                    AddObjects(
                        sheet, hashtags,
                        _ => _.Hashtag.HashtagTitle,
                        _ => _.PostPostTitle
                        );

                });
        }
    }
}