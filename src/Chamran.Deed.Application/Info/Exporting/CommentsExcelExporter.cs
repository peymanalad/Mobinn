using System.Collections.Generic;
using Abp.Runtime.Session;
using Abp.Timing.Timezone;
using Chamran.Deed.DataExporting.Excel.NPOI;
using Chamran.Deed.Info.Dtos;
using Chamran.Deed.Dto;
using Chamran.Deed.Storage;

namespace Chamran.Deed.Info.Exporting
{
    public class CommentsExcelExporter : NpoiExcelExporterBase, ICommentsExcelExporter
    {

        private readonly ITimeZoneConverter _timeZoneConverter;
        private readonly IAbpSession _abpSession;

        public CommentsExcelExporter(
            ITimeZoneConverter timeZoneConverter,
            IAbpSession abpSession,
            ITempFileCacheManager tempFileCacheManager) :
    base(tempFileCacheManager)
        {
            _timeZoneConverter = timeZoneConverter;
            _abpSession = abpSession;
        }

        public FileDto ExportToFile(List<GetCommentForViewDto> comments)
        {
            return CreateExcelPackage(
                "Comments.xlsx",
                excelPackage =>
                {

                    var sheet = excelPackage.CreateSheet(L("Comments"));

                    AddHeader(
                        sheet,
                        L("CommentCaption"),
                        L("InsertDate"),
                        (L("Post")) + L("PostTitle"),
                        (L("User")) + L("Name"),
                        (L("Comment")) + L("CommentCaption")
                        );

                    AddObjects(
                        sheet, comments,
                        _ => _.Comment.CommentCaption,
                        _ => _timeZoneConverter.Convert(_.Comment.InsertDate, _abpSession.TenantId, _abpSession.GetUserId()),
                        _ => _.PostPostTitle,
                        _ => _.UserName,
                        _ => _.CommentCommentCaption
                        );

                    for (var i = 1; i <= comments.Count; i++)
                    {
                        SetCellDataFormat(sheet.GetRow(i).Cells[2], "yyyy-mm-dd");
                    }
                    sheet.AutoSizeColumn(2);
                });
        }
    }
}