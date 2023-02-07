using System.Collections.Generic;
using Chamran.Deed.Auditing.Dto;
using Chamran.Deed.Dto;

namespace Chamran.Deed.Auditing.Exporting
{
    public interface IAuditLogListExcelExporter
    {
        FileDto ExportToFile(List<AuditLogListDto> auditLogListDtos);

        FileDto ExportToFile(List<EntityChangeListDto> entityChangeListDtos);
    }
}
