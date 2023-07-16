using System.Collections.Generic;
using Chamran.Deed.Info.Dtos;
using Chamran.Deed.Dto;

namespace Chamran.Deed.Info.Exporting
{
    public interface IUserLocationsExcelExporter
    {
        FileDto ExportToFile(List<GetUserLocationForViewDto> userLocations);
    }
}