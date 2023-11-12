namespace Chamran.Deed.People.Dtos
{
    public class GetOrganizationForViewDto
    {
        public OrganizationDto Organization { get; set; }
        public string DeedChartCaption { get; set; }
        public string DeedChartParentCaption { get; set; }
    }
}