using Chamran.Deed.Dto;

namespace Chamran.Deed.Authorization.Users.Dto;

public class GetUserInformationDto: PagedAndSortedInputDto
{
    public int UserId { get; set; }
}