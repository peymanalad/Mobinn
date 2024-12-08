using System;
using Abp.Application.Services.Dto;

namespace Chamran.Deed.Info.Dtos
{
    public class UserPostGroupDto : EntityDto
    {

        public long UserId { get; set; }

        public int PostGroupId { get; set; }

    }

    public class AllowedUserPostGroupDto : EntityDto
    {

        public long UserId { get; set; }

        public int PostGroupId { get; set; }

    }
}