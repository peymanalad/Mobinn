﻿using System.Collections.Generic;
using Chamran.Deed.Info.Dtos;
using Chamran.Deed.Dto;

namespace Chamran.Deed.Info.Exporting
{
    public interface IUserPostGroupsExcelExporter
    {
        FileDto ExportToFile(List<GetUserPostGroupForViewDto> userPostGroups);
    }
}