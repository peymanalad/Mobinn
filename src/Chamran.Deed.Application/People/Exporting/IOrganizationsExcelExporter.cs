using System.Collections.Generic;
using Chamran.Deed.People.Dtos;
using Chamran.Deed.Dto;

namespace Chamran.Deed.People.Exporting
{
    public interface IOrganizationsExcelExporter
    {
        FileDto ExportToFile(List<GetOrganizationForViewDto> organizations);
    }
}