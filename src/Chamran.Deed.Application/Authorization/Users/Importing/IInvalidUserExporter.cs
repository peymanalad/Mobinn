using System.Collections.Generic;
using Chamran.Deed.Authorization.Users.Importing.Dto;
using Chamran.Deed.Dto;

namespace Chamran.Deed.Authorization.Users.Importing
{
    public interface IInvalidUserExporter
    {
        FileDto ExportToFile(List<ImportUserDto> userListDtos);
    }
}
