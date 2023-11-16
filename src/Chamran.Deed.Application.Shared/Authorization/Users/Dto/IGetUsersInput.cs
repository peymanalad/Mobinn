using System;
using System.Collections.Generic;
using Abp.Application.Services.Dto;
using Abp.Runtime.Validation;

namespace Chamran.Deed.Authorization.Users.Dto
{
    public interface IGetUsersInput : ISortedResultRequest
    {
        string Filter { get; set; }
        public DateTime? FromCreationDate { get; set; }
        public DateTime? ToCreationDate { get; set; }
        public DateTime? FromLastLoginDate { get; set; }
        public DateTime? ToLastLoginDate { get; set; }
        List<string> Permissions { get; set; }

        int? Role { get; set; }

        bool OnlyLockedUsers { get; set; }
    }
}
