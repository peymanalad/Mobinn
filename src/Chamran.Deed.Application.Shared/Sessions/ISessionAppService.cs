﻿using System.Threading.Tasks;
using Abp.Application.Services;
using Chamran.Deed.Sessions.Dto;

namespace Chamran.Deed.Sessions
{
    public interface ISessionAppService : IApplicationService
    {
        Task<GetCurrentLoginInformationsOutput> GetCurrentLoginInformations();

        Task<UpdateUserSignInTokenOutput> UpdateUserSignInToken();
    }
}
