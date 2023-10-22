namespace Chamran.Deed.People.Dtos
{
    public class GetAllNoOrganizationForViewDto
    {
        public int MemberPos { get; set; }
        public string MemberPosition { get; set; }
        public long? UserId { get; set; }
        public string NationalId { get; set; }
        public string Name { get; set; }
        public string SurName { get; set; }
        public string OrganizationGroupName { get; set; }
    }
}