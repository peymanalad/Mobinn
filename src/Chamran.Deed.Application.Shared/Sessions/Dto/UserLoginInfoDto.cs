﻿using Abp.Application.Services.Dto;
using Chamran.Deed.Authorization.Accounts.Dto;

namespace Chamran.Deed.Sessions.Dto
{
    public class UserLoginInfoDto : EntityDto<long>
    {
        public string Name { get; set; }

        public string Surname { get; set; }

        public string UserName { get; set; }

        public string EmailAddress { get; set; }

        public string ProfilePictureId { get; set; }

        public bool IsSuperUser { get; set; }

        public AccountUserType UserType { get; set; }
    }
}
