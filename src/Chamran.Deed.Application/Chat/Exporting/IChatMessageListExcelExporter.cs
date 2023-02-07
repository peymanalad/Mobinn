using System.Collections.Generic;
using Abp;
using Chamran.Deed.Chat.Dto;
using Chamran.Deed.Dto;

namespace Chamran.Deed.Chat.Exporting
{
    public interface IChatMessageListExcelExporter
    {
        FileDto ExportToFile(UserIdentifier user, List<ChatMessageExportDto> messages);
    }
}
