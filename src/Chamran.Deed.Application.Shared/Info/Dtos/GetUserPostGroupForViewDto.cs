namespace Chamran.Deed.Info.Dtos
{
    public class GetUserPostGroupForViewDto
    {
        public UserPostGroupDto UserPostGroup { get; set; }

        public string UserName { get; set; }

        public string PostGroupPostGroupDescription { get; set; }

    }

    public class GetAllowedUserPostGroupForViewDto
    {
        public AllowedUserPostGroupDto UserPostGroup { get; set; }

        public string UserName { get; set; }

        public string PostGroupPostGroupDescription { get; set; }

    }
}