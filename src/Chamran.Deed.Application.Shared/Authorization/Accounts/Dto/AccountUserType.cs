using System;
using System.Collections.Generic;
using System.Text;

namespace Chamran.Deed.Authorization.Accounts.Dto
{
    public enum AccountUserType
    {
        Normal=0,
        Creator=1,
        Monitor=2,
        Distributer = 3,
        Admin =4,
        SuperAdmin=5
    }
}
