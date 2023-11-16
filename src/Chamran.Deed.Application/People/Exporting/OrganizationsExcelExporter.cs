using System.Collections.Generic;
using Abp.Runtime.Session;
using Abp.Timing.Timezone;
using Chamran.Deed.DataExporting.Excel.NPOI;
using Chamran.Deed.People.Dtos;
using Chamran.Deed.Dto;
using Chamran.Deed.Storage;

namespace Chamran.Deed.People.Exporting
{
    public class OrganizationsExcelExporter : NpoiExcelExporterBase, IOrganizationsExcelExporter
    {

        private readonly ITimeZoneConverter _timeZoneConverter;
        private readonly IAbpSession _abpSession;

        public OrganizationsExcelExporter(
            ITimeZoneConverter timeZoneConverter,
            IAbpSession abpSession,
            ITempFileCacheManager tempFileCacheManager) :
    base(tempFileCacheManager)
        {
            _timeZoneConverter = timeZoneConverter;
            _abpSession = abpSession;
        }

        public FileDto ExportToFile(List<GetOrganizationForViewDto> organizations)
        {
            return CreateExcelPackage(
                    "Organizations.xlsx",
                    excelPackage =>
                    {

                        var sheet = excelPackage.CreateSheet(L("Organizations"));

                        AddHeader(
                            sheet,
                        "نام سازمان",
                        "دولتی؟",
                        "شناسه ملی",
                        "مکان سازمان",
                        "تلفن",
                        "ادمین",
                        "توضیحات",
                        "سلسله مراتب"
                            );

                        AddObjects(
                            sheet, organizations,
                        _ => _.Organization.OrganizationName,
                        _ => _.Organization.IsGovernmental,
                        _ => _.Organization.NationalId,
                        _ => _.Organization.OrganizationLocation,
                        _ => _.Organization.OrganizationPhone,
                        _ => _.Organization.OrganizationContactPerson,
                        _ => _.Organization.Comment,
                        _ => _.LeafCationPath
                            );

                    });

        }
    }
}