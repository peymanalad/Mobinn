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

                    //AddHeader(
                    //    sheet,
                    //    L("PostFile"),
                    //    L("PostCaption"),
                    //    L("IsSpecial"),
                    //    L("PostTitle"),
                    //    (L("GroupMember")) + L("MemberPosition"),
                    //    (L("PostGroup")) + L("PostGroupDescription")
                    //    );
                    AddHeader(
                        sheet,
                        "شرح خبر",
                        "عنوان خبر",
                        "گروه خبر",
                        "تاریخ",
                        "تاریخ میلادی",
                        "منتشر شده",
                        "لینک خبر");

                    AddObjects(
                        sheet, posts,
                        _ => _.Post.PostCaption,
                        _ => _.Post.PostTitle,
                        _ => _.PostGroupPostGroupDescription,
                        _=>_.PersianCreationTime,
                        _=>_.Post.CreationTime,
                        _ => _.Post.IsPublished,
                        _=>_.Post.PostRefLink
                        );

                    for (var i = 1; i <= posts.Count; i++)
                    {
                        SetCellDataFormat(sheet.GetRow(i).Cells[4], "yyyy-mm-dd");
                    }
                    sheet.AutoSizeColumn(1);
                    sheet.AutoSizeColumn(2);
                    sheet.AutoSizeColumn(3);
                    sheet.AutoSizeColumn(4);
                });
        }
    }
}