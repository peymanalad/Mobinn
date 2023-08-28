using Abp.Application.Services.Dto;

namespace Chamran.Deed.Info.Dtos
{
    public class GetAllUsersForLeafInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }
        public int OrganizationChartId { get; set; }
    }
}