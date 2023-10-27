using Abp.Application.Services.Dto;

namespace Chamran.Deed.Info.Dtos
{
    public class GetLeavesInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }
        //public int OrganizationChartId { get; set; }
        public int  OrganizationId { get; set; }
    }
}