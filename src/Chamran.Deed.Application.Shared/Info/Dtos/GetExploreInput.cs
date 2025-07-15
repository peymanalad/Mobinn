using Abp.Application.Services.Dto;

public class GetExploreInput : PagedAndSortedResultRequestDto
{
    public int OrganizationId { get; set; }
    public int? PostGroupId { get; set; }
    public int? PostSubGroupId { get; set; }
}