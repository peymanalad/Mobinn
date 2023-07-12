using System.Collections.Generic;
using Chamran.Deed.Common.Dtos;
using Chamran.Deed.Dto;

namespace Chamran.Deed.Common.Exporting
{
    public interface IUserTokensExcelExporter
    {
        FileDto ExportToFile(List<GetUserTokenForViewDto> userTokens);
    }
}