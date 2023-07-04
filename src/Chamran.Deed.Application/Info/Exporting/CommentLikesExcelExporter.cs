using System.Collections.Generic;
using Abp.Runtime.Session;
using Abp.Timing.Timezone;
using Chamran.Deed.DataExporting.Excel.NPOI;
using Chamran.Deed.Info.Dtos;
using Chamran.Deed.Dto;
using Chamran.Deed.Storage;

namespace Chamran.Deed.Info.Exporting
{
    public class CommentLikesExcelExporter : NpoiExcelExporterBase, ICommentLikesExcelExporter
    {

        private readonly ITimeZoneConverter _timeZoneConverter;
        private readonly IAbpSession _abpSession;

        public CommentLikesExcelExporter(
            ITimeZoneConverter timeZoneConverter,
            IAbpSession abpSession,
            ITempFileCacheManager tempFileCacheManager) :
    base(tempFileCacheManager)
        {
            _timeZoneConverter = timeZoneConverter;
            _abpSession = abpSession;
        }

        public FileDto ExportToFile(List<GetCommentLikeForViewDto> commentLikes)
        {
            return CreateExcelPackage(
                "CommentLikes.xlsx",
                excelPackage =>
                {

                    var sheet = excelPackage.CreateSheet(L("CommentLikes"));

                    AddHeader(
                        sheet,
                        L("LikeTime"),
                        (L("Comment")) + L("CommentCaption"),
                        (L("User")) + L("Name")
                        );

                    AddObjects(
                        sheet, commentLikes,
                        _ => _timeZoneConverter.Convert(_.CommentLike.LikeTime, _abpSession.TenantId, _abpSession.GetUserId()),
                        _ => _.CommentCommentCaption,
                        _ => _.UserName
                        );

                    for (var i = 1; i <= commentLikes.Count; i++)
                    {
                        SetCellDataFormat(sheet.GetRow(i).Cells[1], "yyyy-mm-dd");
                    }
                    sheet.AutoSizeColumn(1);
                });
        }
    }
}