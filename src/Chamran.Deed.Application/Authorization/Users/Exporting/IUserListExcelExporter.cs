using System.Collections.Generic;
using Chamran.Deed.Authorization.Users.Dto;
using Chamran.Deed.Dto;

namespace Chamran.Deed.Authorization.Users.Exporting
{
    public interface IUserListExcelExporter
    {
        FileDto ExportToFile(List<UserListDto> userListDtos);
    }
}