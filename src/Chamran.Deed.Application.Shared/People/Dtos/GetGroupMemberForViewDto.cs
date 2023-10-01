namespace Chamran.Deed.People.Dtos
{
    public class GetGroupMemberForViewDto
    {
        public GroupMemberDto GroupMember { get; set; }

        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string NationalId { get; set; }

        public string OrganizationGroupGroupName { get; set; }

    }
}