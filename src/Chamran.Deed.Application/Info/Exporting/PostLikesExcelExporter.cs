using System.Collections.Generic;
using Abp.Runtime.Session;
using Abp.Timing.Timezone;
using Chamran.Deed.DataExporting.Excel.NPOI;
using Chamran.Deed.Info.Dtos;
using Chamran.Deed.Dto;
using Chamran.Deed.Storage;

namespace Chamran.Deed.Info.Exporting
{
    public class PostLikesExcelExporter : NpoiExcelExporterBase, IPostLikesExcelExporter
    {

        private readonly ITimeZoneConverter _timeZoneConverter;
        private readonly IAbpSession _abpSession;

        public PostLikesExcelExporter(
            ITimeZoneConverter timeZoneConverter,
            IAbpSession abpSession,
            ITempFileCacheManager tempFileCacheManager) :
    base(tempFileCacheManager)
        {
            _timeZoneConverter = timeZoneConverter;
            _abpSession = abpSession;
        }

        public FileDto ExportToFile(List<GetPostLikeForViewDto> postLikes)
        {
            return CreateExcelPackage(
                "PostLikes.xlsx",
                excelPackage =>
                {

                    var sheet = excelPackage.CreateSheet(L("PostLikes"));

                    AddHeader(
                        sheet,
                        L("LikeTime"),
                        (L("Post")) + L("PostTitle"),
                        (L("User")) + L("Name")
                        );

                    AddObjects(
                        sheet, postLikes,
                        _ => _timeZoneConverter.Convert(_.PostLike.LikeTime, _abpSession.TenantId, _abpSession.GetUserId()),
                        _ => _.PostPostTitle,
                        _ => _.UserName
                        );

                    for (var i = 1; i <= postLikes.Count; i++)
                    {
                        SetCellDataFormat(sheet.GetRow(i).Cells[1], "yyyy-mm-dd");
                    }
                    sheet.AutoSizeColumn(1);
                });
        }
    }
}