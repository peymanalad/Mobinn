using System.Collections.Generic;
using Abp.Runtime.Session;
using Abp.Timing.Timezone;
using Chamran.Deed.DataExporting.Excel.NPOI;
using Chamran.Deed.People.Dtos;
using Chamran.Deed.Dto;
using Chamran.Deed.Storage;

namespace Chamran.Deed.People.Exporting
{
    public class GroupMembersExcelExporter : NpoiExcelExporterBase, IGroupMembersExcelExporter
    {

        private readonly ITimeZoneConverter _timeZoneConverter;
        private readonly IAbpSession _abpSession;

        public GroupMembersExcelExporter(
            ITimeZoneConverter timeZoneConverter,
            IAbpSession abpSession,
            ITempFileCacheManager tempFileCacheManager) :
    base(tempFileCacheManager)
        {
            _timeZoneConverter = timeZoneConverter;
            _abpSession = abpSession;
        }

        public FileDto ExportToFile(List<GetGroupMemberForViewDto> groupMembers)
        {
            return CreateExcelPackage(
                "GroupMembers.xlsx",
                excelPackage =>
                {

                    var sheet = excelPackage.CreateSheet(L("GroupMembers"));

                    AddHeader(
                        sheet,
                        L("MemberPosition"),
                        (L("User")) + L("Name"),
                        (L("OrganizationGroup")) + L("GroupName")
                        );

                    AddObjects(
                        sheet, groupMembers,
                        _ => _.GroupMember.MemberPosition,
                        _ => _.UserName,
                        _ => _.OrganizationGroupGroupName
                        );

                });
        }
    }
}