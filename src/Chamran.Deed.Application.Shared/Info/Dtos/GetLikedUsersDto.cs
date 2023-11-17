using System;
using Abp.Application.Services.Dto;
using Abp.Domain.Entities;

namespace Chamran.Deed.Info.Dtos;

public class GetLikedUsersDto 
{
    public string NationalId { get; set; }
    public string Name { get; set; }

    public string Surname { get; set; }

    public string UserName { get; set; }

    public string PhoneNumber { get; set; }

    public Guid? ProfilePictureId { get; set; }

    public bool IsSuperUser { get; set; }
    public DateTime LikeTime { get; set; }
    public long UserId { get; set; }
}