using Abp.Application.Services.Dto;
using Abp.Domain.Entities.Auditing;
using Chamran.Deed.Authorization.Users.Dto;
using System.Collections.Generic;

namespace Chamran.Deed.Info.Dtos;

public class GetLikedUsersInput: PagedAndSortedResultRequestDto
{
    public int PostId { get; set; }
}