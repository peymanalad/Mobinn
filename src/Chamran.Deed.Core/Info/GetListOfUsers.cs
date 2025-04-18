﻿using System;
using Abp.Application.Services.Dto;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Chamran.Deed.Authorization.Accounts.Dto;

namespace Chamran.Deed.Info;

public class GetListOfUsers :EntityDto<long>, IPassivable, IHasCreationTime
{
    public int Id { get; set; }
    public string NationalId { get; set; }
    public string Name { get; set; }

    public string Surname { get; set; }

    public string UserName { get; set; }

    public string EmailAddress { get; set; }

    public string PhoneNumber { get; set; }
    public int UserType{ get; set; }

    public Guid? ProfilePictureId { get; set; }

    //public List<UserListRoleDto> Roles { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreationTime { get; set; }
    public DateTime? LastLoginAttemptTime { get; set; }
    public int? AssignedRoleId { get; set; }
    public string AssignedRoleName { get; set; }
    public DateTime? LockoutEndDate { get; set; }
}