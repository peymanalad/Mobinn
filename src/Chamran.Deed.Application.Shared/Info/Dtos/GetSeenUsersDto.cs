using System;

namespace Chamran.Deed.Info.Dtos;

public class GetSeenUsersDto
{
    public string NationalId { get; set; }
    public string Name { get; set; }

    public string Surname { get; set; }

    public string UserName { get; set; }

    public string PhoneNumber { get; set; }

    public Guid? ProfilePictureId { get; set; }

    public bool IsSuperUser { get; set; }
    public DateTime SeenTime { get; set; }
    public long UserId { get; set; }
}