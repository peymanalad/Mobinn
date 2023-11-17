using Abp.Application.Services.Dto;

namespace Chamran.Deed.Info.Dtos;

public class GetSeenUsersInput : PagedAndSortedResultRequestDto
{
    public int PostId { get; set; }
}