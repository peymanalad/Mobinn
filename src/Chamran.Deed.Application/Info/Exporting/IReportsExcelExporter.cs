using System.Collections.Generic;
using Chamran.Deed.Info.Dtos;
using Chamran.Deed.Dto;

namespace Chamran.Deed.Info.Exporting
{
    public interface IReportsExcelExporter
    {
        FileDto ExportToFile(List<GetReportForViewDto> reports);
    }
}