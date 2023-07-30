using System.Collections.Generic;
using Abp.Runtime.Session;
using Abp.Timing.Timezone;
using Chamran.Deed.DataExporting.Excel.NPOI;
using Chamran.Deed.Info.Dtos;
using Chamran.Deed.Dto;
using Chamran.Deed.Storage;

namespace Chamran.Deed.Info.Exporting
{
    public class PostsExcelExporter : NpoiExcelExporterBase, IPostsExcelExporter
    {

        private readonly ITimeZoneConverter _timeZoneConverter;
        private readonly IAbpSession _abpSession;

        public PostsExcelExporter(
            ITimeZoneConverter timeZoneConverter,
            IAbpSession abpSession,
            ITempFileCacheManager tempFileCacheManager) :
    base(tempFileCacheManager)
        {
            _timeZoneConverter = timeZoneConverter;
            _abpSession = abpSession;
        }

        public FileDto ExportToFile(List<GetPostForViewDto> posts)
        {
            return CreateExcelPackage(
                "Posts.xlsx",
                excelPackage =>
                {

                    var sheet = excelPackage.CreateSheet(L("Posts"));

                    AddHeader(
                        sheet,
                        L("PostFile"),
                        L("PostCaption"),
                        L("IsSpecial"),
                        L("PostTitle"),
                        (L("GroupMember")) + L("MemberPosition"),
                        (L("PostGroup")) + L("PostGroupDescription")
                        );

                    AddObjects(
                        sheet, posts,
                        _ => _.Post.PostFileFileName,
                        _ => _.Post.PostCaption,
                        _ => _.Post.IsSpecial,
                        _ => _.Post.PostTitle,
                        _ => _.GroupMemberMemberPosition,
                        _ => _.PostGroupPostGroupDescription
                        );

                    for (var i = 1; i <= posts.Count; i++)
                    {
                        SetCellDataFormat(sheet.GetRow(i).Cells[3], "yyyy-mm-dd");
                    }
                    sheet.AutoSizeColumn(3);
                });
        }
    }
}