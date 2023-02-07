using System.Collections.Generic;
using Chamran.Deed.Authorization.Users.Importing.Dto;
using Abp.Dependency;

namespace Chamran.Deed.Authorization.Users.Importing
{
    public interface IUserListExcelDataReader: ITransientDependency
    {
        List<ImportUserDto> GetUsersFromExcel(byte[] fileBytes);
    }
}
